using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Onboarding;

/// <summary>Step 1: the business itself.</summary>
public partial class BusinessDetailsStepViewModel : OnboardingStepViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your business name")]
    [MinLength(2, ErrorMessage = "Business name is too short")]
    private string _businessName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your business email")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    private string _businessEmail = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your business phone number")]
    [RegularExpression(@"^\+?[0-9\s\-]{7,20}$", ErrorMessage = "Enter a valid phone number")]
    private string _businessPhone = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your business address")]
    private string _businessAddress = string.Empty;

    public BusinessProfile ToProfile() =>
        new(BusinessName.Trim(), BusinessEmail.Trim(), BusinessPhone.Trim(), BusinessAddress.Trim());
}
