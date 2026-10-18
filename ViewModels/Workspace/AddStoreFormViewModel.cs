using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>
/// The "Add store" form: a name and a place. The store's payment account is added afterwards on
/// the Payments page, once the store exists.
/// </summary>
public sealed partial class AddStoreFormViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter a store name")]
    [MaxLength(50, ErrorMessage = "Keep the name under 50 characters")]
    private string _storeName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter the store's location")]
    [MaxLength(128, ErrorMessage = "Keep the location under 128 characters")]
    private string _location = string.Empty;

    /// <returns>The trimmed name and location, or null while the form still has errors.</returns>
    public (string Name, string Location)? TryGet()
    {
        ValidateAllProperties();
        return HasErrors ? null : (StoreName.Trim(), Location.Trim());
    }
}
