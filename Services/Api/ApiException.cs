namespace Mercury.Desktop.Services.Api;

/// <summary>
/// A call to the Mercury API that did not work. The message is written for the person using the
/// app, so a view model can show it as it is.
/// </summary>
public sealed class ApiException(string message, int? statusCode = null) : Exception(message)
{
    /// <summary>The HTTP status, or null when the server could not be reached at all.</summary>
    public int? StatusCode { get; } = statusCode;
}
