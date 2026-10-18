using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>Records a bank or wallet account the business already uses.</summary>
public sealed partial class LinkAccountFormViewModel : ObservableValidator
{
    public IReadOnlyList<string> Banks => NigerianBanks.Names;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Choose a bank or wallet")]
    private string? _bank;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter the account number")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Use the 10-digit account number")]
    private string _accountNumber = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter the name on the account")]
    private string _accountName = string.Empty;

    /// <returns>The request to send, or null while the form still has errors.</returns>
    public CreateCollectionAccountRequest? TryBuild()
    {
        ValidateAllProperties();
        return HasErrors
            ? null
            : new CreateCollectionAccountRequest(
                "Linked", null, AccountNumber.Trim(), null, null, Bank, AccountName.Trim());
    }
}

/// <summary>
/// Asks Mercury to issue a virtual account. The provider needs the owner's BVN and the bank
/// account it belongs to to verify who is asking; the BVN is passed on and never kept.
/// </summary>
public sealed partial class MercuryAccountFormViewModel : ObservableValidator
{
    public IReadOnlyList<string> Banks => NigerianBanks.Names;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your BVN")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "A BVN has 11 digits")]
    private string _bvn = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Choose the bank your BVN is linked to")]
    private string? _bank;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your account number")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Use the 10-digit account number")]
    private string _accountNumber = string.Empty;

    /// <returns>The request to send, or null while the form still has errors.</returns>
    public CreateCollectionAccountRequest? TryBuild()
    {
        ValidateAllProperties();
        return HasErrors
            ? null
            : new CreateCollectionAccountRequest(
                "MercuryVirtual", Bvn.Trim(), AccountNumber.Trim(), NigerianBanks.CodeFor(Bank), null, Bank, null);
    }
}
