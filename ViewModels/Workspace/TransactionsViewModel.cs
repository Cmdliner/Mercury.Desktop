using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>The activity log: filter by store, type and status, and open one for its detail.</summary>
public sealed partial class TransactionsViewModel : PageViewModel
{
    private const string AllStores = "All stores";
    private const string AllTypes = "All types";
    private const string AllStatuses = "All statuses";

    private readonly IReadOnlyList<Transaction> _all;

    public TransactionsViewModel(IReadOnlyList<Transaction> transactions)
    {
        _all = transactions;
        Stores = [AllStores, .. transactions.Select(t => t.Store).Distinct()];
        Types = [AllTypes, .. transactions.Select(t => t.Type).Distinct()];
        Statuses = [AllStatuses, .. Enum.GetNames<TransactionStatus>()];

        View = CollectionViewSource.GetDefaultView(transactions);
        View.Filter = Matches;
    }

    public override string Title => "Transactions";

    public ICollectionView View { get; }

    public IReadOnlyList<string> Stores { get; }

    public IReadOnlyList<string> Types { get; }

    public IReadOnlyList<string> Statuses { get; }

    [ObservableProperty]
    private string _selectedStore = AllStores;

    [ObservableProperty]
    private string _selectedType = AllTypes;

    [ObservableProperty]
    private string _selectedStatus = AllStatuses;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailOpen))]
    private Transaction? _selected;

    [ObservableProperty]
    private bool _hasResults = true;

    public bool IsDetailOpen => Selected is not null;

    partial void OnSelectedStoreChanged(string value) => Refresh();

    partial void OnSelectedTypeChanged(string value) => Refresh();

    partial void OnSelectedStatusChanged(string value) => Refresh();

    [RelayCommand]
    private void Open(Transaction transaction) => Selected = transaction;

    [RelayCommand]
    private void CloseDetail() => Selected = null;

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedStore = AllStores;
        SelectedType = AllTypes;
        SelectedStatus = AllStatuses;
    }

    private bool Matches(object item) =>
        item is Transaction t
        && (SelectedStore == AllStores || t.Store == SelectedStore)
        && (SelectedType == AllTypes || t.Type == SelectedType)
        && (SelectedStatus == AllStatuses || t.StatusText == SelectedStatus);

    private void Refresh()
    {
        View.Refresh();
        HasResults = _all.Any(t => Matches(t));
    }
}
