using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>
/// The signed-in workspace: the sidebar, the top bar and whichever page is selected. It is
/// built from what onboarding collected, so it knows the business and the owner.
/// </summary>
public sealed partial class WorkspaceViewModel : ObservableObject
{
    public WorkspaceViewModel(OnboardingResult account)
    {
        BusinessName = account.Business.Name;
        OwnerName = account.Owner.FullName;
        OwnerInitials = InitialsOf(account.Owner.FullName);
        OwnerEmail = account.Owner.Email;

        var firstName = account.Owner.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "there";

        // Stores and Payments work on the same list, so adding a store shows up in both.
        var stores = new ObservableCollection<StoreAccount>(
        [
            new(account.Store.Name, account.Store.Location, account.Store.Bank, account.Store.VirtualAccountNumber, 1820400m),
        ]);

        NavItems =
        [
            new("Overview", "Grid", "OPERATIONS", new OverviewViewModel(firstName)),
            new("Stores", "Store", "OPERATIONS", new StoresViewModel(stores)),
            new("Payments", "Payments", "OPERATIONS", new PaymentsViewModel(stores, account.Business.Name)),
            new("Transactions", "Transfer", "OPERATIONS", new TransactionsViewModel(DemoData.Transactions)),
            new("Accounts", "Wallet", "OPERATIONS", new AccountsViewModel(DemoData.Accounts, DemoData.LedgerEntries)),
            new("Staff", "Users", "MANAGEMENT", new StaffViewModel(DemoData.Staff, stores)),
        ];

        // A grouped view lets the sidebar show a heading above each section.
        NavView = CollectionViewSource.GetDefaultView(NavItems);
        NavView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(NavItemViewModel.Section)));

        PrivacySettings.Instance.AmountsHidden = false;
        _selectedNavItem = NavItems[0];
    }

    public string BusinessName { get; }

    public string OwnerName { get; }

    public string OwnerInitials { get; }

    public string OwnerEmail { get; }

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
