using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Onboarding;

/// <summary>Step 3: the first store's name and place. Its payment account is set up in the next step.</summary>
public partial class FirstStoreStepViewModel : OnboardingStepViewModel
{
    public override string ContinueLabel => "Create store";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter a store name")]
    [MinLength(2, ErrorMessage = "Store name is too short")]
    [MaxLength(50, ErrorMessage = "Keep the name under 50 characters")]
    private string _storeName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter the store location")]
    [MaxLength(128, ErrorMessage = "Keep the location under 128 characters")]
    private string _location = string.Empty;

    public StoreProfile ToProfile() => new(StoreName.Trim(), Location.Trim());
}
