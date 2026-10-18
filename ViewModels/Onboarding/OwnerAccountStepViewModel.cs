using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services;

namespace Mercury.Desktop.ViewModels.Onboarding;

/// <summary>Step 2: the owner's own account.</summary>
public partial class OwnerAccountStepViewModel : OnboardingStepViewModel
{
    public override string ContinueLabel => "Create account";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your full name")]
    [MinLength(2, ErrorMessage = "Name is too short")]
    private string _fullName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your email address")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    private string _emailAddress = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Create a password")]
    [MinLength(8, ErrorMessage = "Use at least 8 characters")]
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
        if (!string.IsNullOrEmpty(ConfirmPassword))
        {
            ValidateProperty(ConfirmPassword, nameof(ConfirmPassword));
        }
    }

    public OwnerAccount ToAccount() => new(FullName.Trim(), EmailAddress.Trim(), Password);
}
