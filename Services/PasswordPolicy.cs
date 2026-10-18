namespace Mercury.Desktop.Services;

/// <summary>
/// The rule the server applies to every password, so a form can say so before sending it instead
/// of after the server refuses it. Keep in step with the Identity options in Mercury.Api's Program.cs.
/// </summary>
public static class PasswordPolicy
{
    /// <summary>At least 6 characters with a capital letter, a number and a symbol.</summary>
    public const string Pattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$";

    public const string Message = "Use a capital letter, a number and a symbol";
}
