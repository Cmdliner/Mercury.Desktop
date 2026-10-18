using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>The business's accounts and the latest movements between them.</summary>
public sealed partial class AccountsViewModel(IReadOnlyList<LedgerAccount> accounts, IReadOnlyList<LedgerEntry> entries)
    : PageViewModel
{
    public override string Title => "Accounts";

    public IReadOnlyList<LedgerAccount> Accounts { get; } = accounts;

    public IReadOnlyList<LedgerEntry> Entries { get; } = entries;

    public decimal Total { get; } = accounts.Sum(a => a.Balance);

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
}
