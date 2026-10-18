using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>The "Add store" form: a name, a place and the virtual account for the new store.</summary>
public sealed partial class AddStoreFormViewModel : ObservableValidator
{
    // TODO: the virtual account and its bank will be issued by the backend; for now they are typed in.
    private const string DefaultBank = "GTBank";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter a store name")]
    private string _storeName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter the store's location")]
    private string _location = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter the virtual account number")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Use the 10-digit account number")]
    private string _virtualAccountNumber = string.Empty;

    /// <returns>The new store, or null when the form still has errors.</returns>
    public StoreAccount? TryCreate()
    {
        ValidateAllProperties();
        return HasErrors
            ? null
            : new StoreAccount(StoreName.Trim(), Location.Trim(), DefaultBank, VirtualAccountNumber.Trim(), 0m);
    }
}
