using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>
/// The activity log, read from the ledger. The store and type filters ask the server again; the
/// status filter narrows what is already here. Opening a row loads the ledger entries behind it.
/// </summary>
public sealed partial class TransactionsViewModel : PageViewModel
{
    private const string AllStores = "All stores";
    private const string AllTypes = "All types";
    private const string AllStatuses = "All statuses";

    private readonly IMercuryApi _api;
    private readonly StoreDirectory _directory;

    // Filters change quickly; only the newest request's answer is used.
    private int _loadVersion;

    public TransactionsViewModel(IMercuryApi api, StoreDirectory directory)
    {
        _api = api;
        _directory = directory;

        Types = [AllTypes, "Sale", "Refund", "Settlement", "Transfer", "Adjustment"];
        Statuses = [AllStatuses, .. Enum.GetNames<TransactionStatus>()];
        _stores = [AllStores];

        View = CollectionViewSource.GetDefaultView(Items);
        View.Filter = MatchesStatus;

        // The store filter lists the stores, which load alongside this page.
        directory.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(StoreDirectory.IsLoading) && !directory.IsLoading) RefreshStoreChoices();
        };
        RefreshStoreChoices();
    }

    public override string Title => "Transactions";

    public ObservableCollection<Transaction> Items { get; } = [];

    public ICollectionView View { get; }

    [ObservableProperty]
    private IReadOnlyList<string> _stores;

    public IReadOnlyList<string> Types { get; }

    public IReadOnlyList<string> Statuses { get; }

    [ObservableProperty]
    private string _selectedStore = AllStores;

    [ObservableProperty]
    private string _selectedType = AllTypes;

    [ObservableProperty]
    private string _selectedStatus = AllStatuses;

    // ---- loading ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowLoading))]
    private bool _isLoading = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ShowError))]
    private string? _errorMessage;

    /// <summary>True once any list has arrived, so later problems do not hide what is already on screen.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowLoading))]
    [NotifyPropertyChangedFor(nameof(ShowError))]
    [NotifyPropertyChangedFor(nameof(ShowContent))]
    private bool _hasLoadedOnce;

    public bool HasError => ErrorMessage is not null;

    /// <summary>The first load, before there is anything to show.</summary>
    public bool ShowLoading => IsLoading && !HasLoadedOnce;

    /// <summary>The first load failed, so there is nothing to show but the error.</summary>
    public bool ShowError => HasError && !HasLoadedOnce;

    public bool ShowContent => HasLoadedOnce;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyTitle))]
    [NotifyPropertyChangedFor(nameof(EmptyMessage))]
    [NotifyPropertyChangedFor(nameof(HasNoResults))]
    private bool _hasResults;

    public bool HasNoResults => !HasResults;

    private bool FiltersActive => SelectedStore != AllStores || SelectedType != AllTypes || SelectedStatus != AllStatuses;

    public string EmptyTitle => FiltersActive ? "Nothing matches" : "No transactions yet";

    public string EmptyMessage => FiltersActive
        ? "Try a different store, type or status."
        : "Payments will appear here as they arrive.";

    [RelayCommand]
    public async Task LoadAsync()
    {
        var version = ++_loadVersion;
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Guid? storeId = SelectedStore == AllStores
                ? null
                : _directory.Stores.FirstOrDefault(s => s.Name == SelectedStore)?.Id;
            var type = SelectedType == AllTypes ? null : SelectedType;

            var list = await _api.GetTransactionsAsync(storeId, type);
            if (version != _loadVersion) return;

            Items.Clear();
            foreach (var item in list.Items) Items.Add(ToTransaction(item));
            HasLoadedOnce = true;
        }
        catch (ApiException ex)
        {
            if (version == _loadVersion) ErrorMessage = ex.Message;
        }
        finally
        {
            if (version == _loadVersion)
            {
                IsLoading = false;
                UpdateResults();
            }
        }
    }

    partial void OnSelectedStoreChanged(string value)
    {
        // A combo box briefly writes null while its choices are replaced.
        if (value is null) return;
        OnFilterChanged(reload: true);
    }

    partial void OnSelectedTypeChanged(string value)
    {
        if (value is null) return;
        OnFilterChanged(reload: true);
    }

    partial void OnSelectedStatusChanged(string value)
    {
        if (value is null) return;
        OnFilterChanged(reload: false);
    }

    private void OnFilterChanged(bool reload)
    {
        OnPropertyChanged(nameof(EmptyTitle));
        OnPropertyChanged(nameof(EmptyMessage));

        if (reload)
        {
            _ = LoadAsync();
        }
        else
        {
            View.Refresh();
            UpdateResults();
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedStore = AllStores;
        SelectedType = AllTypes;
        SelectedStatus = AllStatuses;
    }

    // ---- detail ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailOpen))]
    private Transaction? _selected;

    public bool IsDetailOpen => Selected is not null;

    [RelayCommand]
    private async Task OpenAsync(Transaction transaction)
    {
        // The drawer opens at once with what the list knows; the ledger entries fill in after.
        Selected = transaction;
        if (transaction.Id == Guid.Empty) return;

        string debit, credit;
        try
        {
            var detail = await _api.GetTransactionAsync(transaction.Id);
            debit = detail.Lines.FirstOrDefault(l => l.Direction == "Debit")?.AccountName ?? "-";
            credit = detail.Lines.FirstOrDefault(l => l.Direction == "Credit")?.AccountName ?? "-";
        }
        catch (ApiException)
        {
            debit = credit = "Unavailable";
        }

        // The person may have closed it, or opened another, while this loaded.
        if (Selected?.Id == transaction.Id) Selected = transaction with { DebitAccount = debit, CreditAccount = credit };
    }

    [RelayCommand]
    private void CloseDetail() => Selected = null;

    // ---- helpers ----

    private bool MatchesStatus(object item) =>
        item is Transaction t && (SelectedStatus == AllStatuses || t.StatusText == SelectedStatus);

    private void UpdateResults() => HasResults = View.Cast<object>().Any();

    private void RefreshStoreChoices()
    {
        var keep = SelectedStore;
        Stores = [AllStores, .. _directory.Stores.Select(s => s.Name)];
        SelectedStore = Stores.Contains(keep) ? keep : AllStores;
    }

    private static Transaction ToTransaction(TransactionResponse t) => new(
        t.Reference,
        t.StoreName ?? "Business",
        t.Type,
        t.Amount,
        Enum.TryParse<TransactionStatus>(t.Status, out var status) ? status : TransactionStatus.Completed,
        t.Method switch
        {
            "BankTransfer" => "Bank transfer",
            "PosCard" => "Card",
            var other => other,
        },
        string.IsNullOrWhiteSpace(t.Description) ? "-" : t.Description,
        DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc).ToLocalTime(),
        string.Empty,
        string.Empty,
        t.Id);
}
