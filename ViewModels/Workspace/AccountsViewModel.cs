using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>The accounts that hold the business's money and the latest movements between them, read from the ledger.</summary>
public sealed partial class AccountsViewModel(IMercuryApi api) : PageViewModel
{
    public override string Title => "Accounts";

    [ObservableProperty]
    private IReadOnlyList<LedgerAccount> _accounts = [];

    [ObservableProperty]
    private IReadOnlyList<LedgerEntry> _entries = [];

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoaded))]
    [NotifyPropertyChangedFor(nameof(HasNoAccounts))]
    private bool _isLoading = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoaded))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(HasNoAccounts))]
    private string? _errorMessage;

    public bool HasError => ErrorMessage is not null;

    public bool IsLoaded => !IsLoading && !HasError;

    /// <summary>Loaded fine but nothing to show, such as a business that has no stores yet.</summary>
    public bool HasNoAccounts => IsLoaded && Accounts.Count == 0;

    public bool HasEntries => Entries.Count > 0;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await api.GetAccountsAsync();
            Total = response.Total;
            Accounts = response.Accounts.Select(ToAccount).ToList();
            Entries = response.RecentEntries.Select(ToEntry).ToList();
            OnPropertyChanged(nameof(HasEntries));
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasNoAccounts));
        }
    }

    // ---- detail ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAccountOpen))]
    private LedgerAccount? _selectedAccount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEntryOpen))]
    private LedgerEntry? _selectedEntry;

    public bool IsAccountOpen => SelectedAccount is not null;

    public bool IsEntryOpen => SelectedEntry is not null;

    [RelayCommand]
    private void OpenAccount(LedgerAccount account) => SelectedAccount = account;

    [RelayCommand]
    private void OpenEntry(LedgerEntry entry) => SelectedEntry = entry;

    [RelayCommand]
    private void CloseDetail()
    {
        SelectedAccount = null;
        SelectedEntry = null;
    }

    // ---- helpers ----

    private static LedgerAccount ToAccount(AccountBalanceResponse a)
    {
        // The code ends in what the account is for; it decides the words and the icon.
        var (description, icon) = a.Code switch
        {
            var c when c.EndsWith("-CLEARING", StringComparison.Ordinal) => ("Collected by the provider, not settled yet", "Transfer"),
            var c when c.EndsWith("-OPERATING", StringComparison.Ordinal) => ("Settled money in your bank", "Wallet"),
            var c when c.EndsWith("-RESERVE", StringComparison.Ordinal) => ("Set aside for later", "Check"),
            var c when c.EndsWith("-CASH-TILL", StringComparison.Ordinal) => ("Cash taken at the till", "Store"),
            _ => ("Holds money for the business", "Wallet"),
        };

        return new LedgerAccount(a.Name, description, a.Balance, icon, Math.Clamp((double)a.Share, 0, 1));
    }

    // Money moves from the credited account to the debited one.
    private static LedgerEntry ToEntry(LedgerEntryResponse e) => new(
        DateTime.SpecifyKind(e.Date, DateTimeKind.Utc).ToLocalTime().ToString("MMM d"),
        string.IsNullOrWhiteSpace(e.Description) ? e.Reference : e.Description,
        e.CreditAccount,
        e.DebitAccount,
        e.Amount);
}
