using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>The "Add staff" form. The role decides what the person can see.</summary>
public sealed partial class InviteStaffFormViewModel(IReadOnlyList<string> stores) : ObservableValidator
{
    public IReadOnlyList<string> Roles { get; } = ["Manager", "Cashier", "Accountant"];

    public IReadOnlyList<string> Stores { get; } = stores;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter their name")]
    private string _fullName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter their email address")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    private string _emailAddress = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Choose a role")]
    private string? _role;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Choose a store")]
    private string? _store;

    /// <returns>The new staff member, or null when the form still has errors.</returns>
    public StaffMember? TryCreate()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            return null;
        }

        var access = Role switch
        {
            "Manager" => "Full",
            "Accountant" => "Financial",
            _ => "Store",
        };
        return new StaffMember(FullName.Trim(), EmailAddress.Trim(), Role!, Store!, access);
    }
}
