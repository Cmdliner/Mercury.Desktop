using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.ViewModels.Login;

/// <summary>The sign-in screen. It validates the form, signs in through the API and reports the result.</summary>
public sealed partial class LoginViewModel(IMercuryApi api) : ObservableValidator
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

    /// <summary>Why the last sign-in did not work, or a notice such as an expired session.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMessage))]
    private string? _message;

    public bool HasMessage => !string.IsNullOrEmpty(Message);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    [NotifyPropertyChangedFor(nameof(SignInLabel))]
    private bool _isBusy;

    public string SignInLabel => IsBusy ? "Signing in..." : "Sign in";

    /// <summary>Raised with the sign-in token once the server has accepted the credentials.</summary>
    public event EventHandler<string>? SignedIn;

    /// <summary>Raised when the person has no account yet and wants to set one up.</summary>
    public event EventHandler? CreateAccountRequested;

    [RelayCommand(CanExecute = nameof(CanSignIn))]
    private async Task SignInAsync()
    {
        Message = null;
        ValidateAllProperties();
        if (HasErrors) return;

        IsBusy = true;
        try
        {
            var token = await api.LoginAsync(EmailAddress, Password);
            Password = string.Empty;
            SignedIn?.Invoke(this, token);
        }
        catch (ApiException ex)
        {
            Message = ex.StatusCode == 401 ? "That email or password is not right." : ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSignIn() => !IsBusy;

    /// <summary>Raised when the person was invited and wants to join their team.</summary>
    public event EventHandler? InvitationRequested;

    [RelayCommand]
    private void HaveInvitation() => InvitationRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void CreateAccount() => CreateAccountRequested?.Invoke(this, EventArgs.Empty);
}
