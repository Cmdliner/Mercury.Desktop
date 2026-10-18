using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>
/// The signed-in workspace: the sidebar, the top bar and whichever page is selected. It is
/// built from what onboarding collected, so it knows the business and the owner.
/// </summary>
public sealed partial class WorkspaceViewModel : ObservableObject
{
    public WorkspaceViewModel(MeResponse me, IMercuryApi api)
    {
        // Pages that are not read from the server yet still show built-in sample figures. Against a
        // real server they say so; in sample mode everything is sample anyway.
        var isLive = !api.IsDemo;

        BusinessName = me.BusinessName;
        OwnerName = me.Name;
        OwnerInitials = InitialsOf(me.Name);
        OwnerEmail = me.Email;
        Role = me.Role;

        var firstName = me.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "there";

        // Stores, Payments and the staff invitation form share one list of stores, read from the server.
        var stores = new StoreDirectory(api);

        var transactions = new TransactionsViewModel(api, stores);
        var accounts = new AccountsViewModel(api);
        var staff = new StaffViewModel(api, stores, me) { CanInvite = Permissions.CanInviteStaff(me.Role) };

        List<NavItemViewModel> pages =
        [
            new("Overview", "Grid", "OPERATIONS", new OverviewViewModel(firstName, api)),
            new("Stores", "Store", "OPERATIONS",
                new StoresViewModel(stores) { CanAddStore = Permissions.CanCreateStores(me.Role) }),
            new("Payments", "Payments", "OPERATIONS",
                new PaymentsViewModel(stores, me.BusinessName) { CanAddAccount = Permissions.CanCreateStores(me.Role) }),
            new("Transactions", "Transfer", "OPERATIONS", transactions),
            new("Accounts", "Wallet", "OPERATIONS", accounts),
            new("Staff", "Users", "MANAGEMENT", staff),
        ];

        // Each role only gets the pages it can use.
        NavItems = pages.Where(p => Permissions.CanSee(me.Role, p.Title)).ToList();

        // A grouped view lets the sidebar show a heading above each section.
        NavView = CollectionViewSource.GetDefaultView(NavItems);
        NavView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(NavItemViewModel.Section)));

        PrivacySettings.Instance.AmountsHidden = false;
        _selectedNavItem = NavItems[0];

        // The overview reads from the server; start it now so it is ready when the page opens.
        _ = ((OverviewViewModel)pages[0].Page).LoadCommand.ExecuteAsync(null);
        _ = stores.LoadAsync();
        _ = transactions.LoadAsync();
        // Cashiers have no Accounts page, and the server would refuse them.
        if (Permissions.CanSee(me.Role, Permissions.Accounts)) _ = accounts.LoadAsync();
        // The server refuses the staff list to accountants and cashiers, who have no Staff page either.
        if (Permissions.CanSee(me.Role, Permissions.Staff)) _ = staff.LoadAsync();
    }

    public string BusinessName { get; }

    public string OwnerName { get; }

    public string OwnerInitials { get; }

    public string OwnerEmail { get; }

    public StaffRole Role { get; }

    public string RoleLabel => Role.ToString();

    public string RoleDescription => Role switch
    {
        StaffRole.Owner => "Owner, full access",
        StaffRole.Manager => "Manager of one store",
        StaffRole.Accountant => "Accountant, read-only",
        _ => "Cashier at one store",
    };

    /// <summary>Raised once the owner has confirmed they want to sign out.</summary>
    public event EventHandler? SignedOut;

    [ObservableProperty]
    private bool _isUserMenuOpen;

    [ObservableProperty]
    private bool _isProfileOpen;

    [ObservableProperty]
    private bool _isSignOutConfirming;

    [RelayCommand]
    private void OpenUserMenu() => IsUserMenuOpen = true;

    [RelayCommand]
    private void ShowProfile()
    {
        IsUserMenuOpen = false;
        IsProfileOpen = true;
    }

    [RelayCommand]
    private void CloseProfile() => IsProfileOpen = false;

    [RelayCommand]
    private void RequestSignOut()
    {
        IsUserMenuOpen = false;
        IsProfileOpen = false;
        IsSignOutConfirming = true;
    }

    [RelayCommand]
    private void CancelSignOut() => IsSignOutConfirming = false;

    [RelayCommand]
    private void ConfirmSignOut()
    {
        IsSignOutConfirming = false;
        SignedOut?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void HideAmountsFromMenu()
    {
        IsUserMenuOpen = false;
        IsAmountsHidden = !IsAmountsHidden;
    }

    public IReadOnlyList<NavItemViewModel> NavItems { get; }

    /// <summary>The nav items grouped by section, for the sidebar list.</summary>
    public ICollectionView NavView { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPage))]
    private NavItemViewModel _selectedNavItem;

    /// <summary>When true, money amounts across the app are masked.</summary>
    [ObservableProperty]
    private bool _isAmountsHidden;

    partial void OnIsAmountsHiddenChanged(bool value) => PrivacySettings.Instance.AmountsHidden = value;

    [RelayCommand]
    private void ToggleAmounts() => IsAmountsHidden = !IsAmountsHidden;

    /// <summary>When true the sidebar shrinks to icons only.</summary>
    [ObservableProperty]
    private bool _isSidebarCollapsed;

    [RelayCommand]
    private void ToggleSidebar() => IsSidebarCollapsed = !IsSidebarCollapsed;

    public PageViewModel CurrentPage => SelectedNavItem.Page;

    private static string InitialsOf(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..1].ToUpperInvariant(),
            _ => $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant(),
        };
    }
}
