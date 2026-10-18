using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;

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

        var firstName = account.Owner.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "there";

        NavItems =
        [
            new("Overview", "Grid", "OPERATIONS", new OverviewViewModel(firstName)),
            new("Stores", "Store", "OPERATIONS",
                new PlaceholderPageViewModel("Stores", "Every branch, its balance and its status, in one list.")),
            new("Transactions", "Transfer", "OPERATIONS",
                new PlaceholderPageViewModel("Transactions", "A full activity log you can filter by store and account.")),
            new("Accounts", "Wallet", "OPERATIONS",
                new PlaceholderPageViewModel("Accounts", "Operating, settlement and spending accounts, with balances from the ledger.")),
            new("Staff", "Users", "MANAGEMENT",
                new PlaceholderPageViewModel("Staff", "Invite your team and place them in the stores they work in.")),
        ];

        // A grouped view lets the sidebar show a heading above each section.
        NavView = CollectionViewSource.GetDefaultView(NavItems);
        NavView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(NavItemViewModel.Section)));

        _selectedNavItem = NavItems[0];
    }

    public string BusinessName { get; }

    public string OwnerName { get; }

    public string OwnerInitials { get; }

    public IReadOnlyList<NavItemViewModel> NavItems { get; }

    /// <summary>The nav items grouped by section, for the sidebar list.</summary>
    public ICollectionView NavView { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPage))]
    private NavItemViewModel _selectedNavItem;

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
