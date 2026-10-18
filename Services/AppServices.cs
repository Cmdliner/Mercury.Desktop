using System.Net.Http;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.Services;

/// <summary>
/// Builds the pieces the app is made of. The server address comes from the MERCURY_API_URL
/// environment variable and defaults to a local development server.
/// </summary>
public static class AppServices
{
    private const string DefaultApiUrl = "http://localhost:5100/";

    /// <summary>When true the app uses built-in sample data instead of a server. Set by the developer switches.</summary>
    public static bool UseDemoData { get; set; }

    /// <summary>
    /// The token of an invitation link the app was opened with, until the shell has used it.
    /// </summary>
    public static string? PendingInvitationToken { get; set; }

    public static IMercuryApi CreateApi()
    {
        if (UseDemoData) return new DemoMercuryApi();

        var address = Environment.GetEnvironmentVariable("MERCURY_API_URL");
        if (string.IsNullOrWhiteSpace(address)) address = DefaultApiUrl;
        if (!address.EndsWith('/')) address += "/";

        var http = new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(30) };
        return new HttpMercuryApi(new ApiClient(http));
    }
}
