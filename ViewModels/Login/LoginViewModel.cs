using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mercury.Desktop.ViewModels.Login;

/// <summary>The sign-in screen. It validates the form and reports what the person chose to do.</summary>
public sealed partial class LoginViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your email address")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    private string _emailAddress = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your password")]
    private string _password = string.Empty;

    /// <summary>Raised once the form is valid and the person pressed Sign in.</summary>
    public event EventHandler? SignedIn;

    /// <summary>Raised when the person has no account yet and wants to set one up.</summary>
    public event EventHandler? CreateAccountRequested;

    [RelayCommand]
    private void SignIn()
    {
        ValidateAllProperties();
        if (!HasErrors)
        {
            // TODO: check the credentials against the Mercury backend.
            SignedIn?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private void CreateAccount() => CreateAccountRequested?.Invoke(this, EventArgs.Empty);
}
