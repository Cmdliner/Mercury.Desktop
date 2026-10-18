using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>
/// The "Add staff" form. The role decides the rest: managers and cashiers belong to one store, an
/// accountant works across them all. A manager inviting a cashier is locked to their own store.
/// </summary>
public sealed partial class InviteStaffFormViewModel : ObservableValidator
{
    private const string AllStores = "All stores";

    private readonly string? _lockedStore;

    public InviteStaffFormViewModel(IReadOnlyList<string> roles, IReadOnlyList<string> storeNames, string? lockedStore = null)
    {
        Roles = roles;
        Stores = storeNames;
        _lockedStore = lockedStore;
        _store = lockedStore;
        if (roles.Count == 1) _role = roles[0];
    }

    public IReadOnlyList<string> Roles { get; }

    public IReadOnlyList<string> Stores { get; }

    /// <summary>True when the person inviting can only invite to their own store.</summary>
    public bool IsStoreLocked => _lockedStore is not null;

    public bool IsStoreEditable => !IsStoreLocked;

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
    [NotifyPropertyChangedFor(nameof(NeedsStore))]
    [Required(ErrorMessage = "Choose a role")]
    private string? _role;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Choose a store")]
    private string? _store;

    /// <summary>An accountant is not tied to a store, so the store choice is hidden for them.</summary>
    public bool NeedsStore => Role != nameof(StaffRole.Accountant);

    partial void OnRoleChanged(string? value)
    {
        if (value == nameof(StaffRole.Accountant)) Store = AllStores;
        else if (Store == AllStores) Store = _lockedStore;
    }

    /// <summary>Builds the invitation, or returns null while the form still has errors.</summary>
    /// <param name="storeIdFor">Finds a store's id from the name chosen.</param>
    public InviteStaffRequest? TryBuild(Func<string, Guid?> storeIdFor)
    {
        ValidateAllProperties();
        if (HasErrors) return null;

        var role = Enum.Parse<StaffRole>(Role!);
        Guid? storeId = role == StaffRole.Accountant ? null : storeIdFor(Store!);
        return new InviteStaffRequest(EmailAddress.Trim(), FullName.Trim(), role, storeId);
    }
}
