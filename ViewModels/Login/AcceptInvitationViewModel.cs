using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Services;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.ViewModels.Login;

/// <summary>
/// Joining a business from an invitation: the invited person chooses their own password. When the
/// app was opened from the invitation link the token is already known and the code box is hidden.
/// </summary>
public sealed partial class AcceptInvitationViewModel : ObservableValidator
{
    private readonly IMercuryApi _api;

    public AcceptInvitationViewModel(IMercuryApi api, string? token = null)
    {
        _api = api;
        _invitation = token ?? string.Empty;
        AsksForCode = string.IsNullOrWhiteSpace(token);
    }

    /// <summary>True when the person has to paste their invitation; false when the link already brought it.</summary>
    public bool AsksForCode { get; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Paste the link or code from your invitation")]
    private string _invitation;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Choose a password")]
    [MinLength(6, ErrorMessage = "Use at least 6 characters")]
    [RegularExpression(PasswordPolicy.Pattern, ErrorMessage = PasswordPolicy.Message)]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Repeat your password")]
    [property: Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    private string _confirmPassword = string.Empty;

    // Changing the password can make an already-typed confirmation valid or invalid.
    partial void OnPasswordChanged(string value)
    {
        if (!string.IsNullOrEmpty(ConfirmPassword)) ValidateProperty(ConfirmPassword, nameof(ConfirmPassword));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMessage))]
    private string? _message;

    public bool HasMessage => !string.IsNullOrEmpty(Message);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    [NotifyPropertyChangedFor(nameof(AcceptLabel))]
    private bool _isBusy;

    public string AcceptLabel => IsBusy ? "Joining..." : "Join the team";

    /// <summary>Raised with the sign-in token once the invitation is accepted.</summary>
    public event EventHandler<string>? Accepted;

    /// <summary>Raised when the person would rather go back to signing in.</summary>
    public event EventHandler? BackRequested;

    [RelayCommand(CanExecute = nameof(CanAccept))]
    private async Task AcceptAsync()
    {
        Message = null;
        ValidateAllProperties();
        if (HasErrors) return;

        IsBusy = true;
        try
        {
            var token = await _api.AcceptInvitationAsync(InvitationLink.ExtractToken(Invitation), Password);
            Password = ConfirmPassword = string.Empty;
            Accepted?.Invoke(this, token);
        }
        catch (ApiException ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanAccept() => !IsBusy;

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);
}
