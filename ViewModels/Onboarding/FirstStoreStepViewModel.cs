using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Onboarding;

/// <summary>Step 3: the first store and where its payments are recorded.</summary>
public partial class FirstStoreStepViewModel : OnboardingStepViewModel
{
    public override string ContinueLabel => "Create store";

    public IReadOnlyList<string> Banks { get; } =
    [
        "Access Bank",
        "Ecobank",
        "Fidelity Bank",
        "First Bank of Nigeria",
        "FCMB",
        "GTBank",
        "Keystone Bank",
        "Polaris Bank",
        "Providus Bank",
        "Stanbic IBTC",
        "Sterling Bank",
        "UBA",
        "Union Bank",
        "Wema Bank",
        "Zenith Bank",
    ];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter a store name")]
    [MinLength(2, ErrorMessage = "Store name is too short")]
    private string _storeName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter the store location")]
    private string _location = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Select a bank")]
    private string? _bank;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter the virtual account number")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter the 10-digit account number")]
    private string _virtualAccountNumber = string.Empty;

    public StoreProfile ToProfile() =>
        new(StoreName.Trim(), Location.Trim(), Bank ?? string.Empty, VirtualAccountNumber.Trim());
}
