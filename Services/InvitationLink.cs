namespace Mercury.Desktop.Services;

/// <summary>
/// Reads an invitation token out of what a person pastes or what the operating system hands the
/// app: a whole link such as <c>mercury://accept-invitation?token=abc</c>, or the bare token.
/// </summary>
public static class InvitationLink
{
    private const string Scheme = "mercury";

    /// <summary>The token in a link or a pasted string, trimmed. A bare token comes back as it is.</summary>
    public static string ExtractToken(string text)
    {
        var trimmed = text.Trim();
        const string key = "token=";

        var start = trimmed.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (start < 0) return trimmed;

        var token = trimmed[(start + key.Length)..];
        var end = token.IndexOf('&');
        if (end >= 0) token = token[..end];
        return Uri.UnescapeDataString(token.Trim());
    }

    /// <summary>The token of an invitation link in the command line, or null if there is none.</summary>
    public static string? FromCommandLine(IEnumerable<string> args)
    {
        foreach (var arg in args)
        {
            if (Uri.TryCreate(arg, UriKind.Absolute, out var uri) &&
                uri.Scheme.Equals(Scheme, StringComparison.OrdinalIgnoreCase))
            {
                var token = ExtractToken(arg);
                if (!string.IsNullOrWhiteSpace(token) && token != arg.Trim()) return token;
            }
        }

        return null;
    }
}
