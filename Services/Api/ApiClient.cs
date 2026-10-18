using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mercury.Desktop.Services.Api;

/// <summary>
/// Talks to the Mercury API: adds the sign-in token, reads the JSON, and turns every failure into
/// an <see cref="ApiException"/> with a message fit to show. It knows nothing about screens.
/// </summary>
public sealed class ApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private string? _token;

    /// <summary>Raised when the server rejects the token, so the app can send the person back to sign in.</summary>
    public event EventHandler? Unauthorized;

    public void SetToken(string? token)
    {
        _token = token;
        http.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    public Task<T> GetAsync<T>(string path, CancellationToken ct = default) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, path), ct);

    public Task<T> PostAsync<T>(string path, object body, CancellationToken ct = default) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body, options: Json),
        }, ct);

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken ct)
    {
        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (ct.IsCancellationRequested) throw;
            throw new ApiException("Can't reach Mercury right now. Check your connection and try again.");
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var value = await response.Content.ReadFromJsonAsync<T>(Json, ct);
                    return value ?? throw new ApiException("Mercury sent a reply the app could not read.");
                }
                catch (Exception ex) when (ex is JsonException or NotSupportedException)
                {
                    throw new ApiException("Mercury sent a reply the app could not read.");
                }
            }

            var message = await ReadErrorAsync(response, ct);

            // A token the server refuses means the session is over. Without a token a 401 is just a wrong password.
            if (response.StatusCode == HttpStatusCode.Unauthorized && _token is not null)
            {
                Unauthorized?.Invoke(this, EventArgs.Empty);
                throw new ApiException("Your session has expired. Please sign in again.", 401);
            }

            throw new ApiException(message, (int)response.StatusCode);
        }
    }

    /// <summary>Reads the server's reason: an <c>error</c>, a list of <c>errors</c>, or standard problem details.</summary>
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var doc = await response.Content.ReadFromJsonAsync<JsonElement>(Json, ct);
            if (doc.ValueKind == JsonValueKind.Object)
            {
                if (doc.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
                    return error.GetString()!;

                if (doc.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
                {
                    var lines = errors.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString()!)
                        .ToList();
                    if (lines.Count > 0) return string.Join(" ", lines);
                }

                foreach (var name in new[] { "detail", "title" })
                {
                    if (doc.TryGetProperty(name, out var text) && text.ValueKind == JsonValueKind.String)
                        return text.GetString()!;
                }
            }
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // The body was not JSON; fall through to the generic message.
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Forbidden => "You do not have access to that.",
            HttpStatusCode.NotFound => "That could not be found.",
            >= HttpStatusCode.InternalServerError => "Mercury had a problem. Please try again in a moment.",
            _ => "Something went wrong. Please try again.",
        };
    }
}
