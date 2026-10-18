using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>The business's accounts and the latest movements between them.</summary>
public sealed class AccountsViewModel(IReadOnlyList<LedgerAccount> accounts, IReadOnlyList<LedgerEntry> entries)
    : PageViewModel
{
    public override string Title => "Accounts";

    public IReadOnlyList<LedgerAccount> Accounts { get; } = accounts;

    public IReadOnlyList<LedgerEntry> Entries { get; } = entries;

    public decimal Total { get; } = accounts.Sum(a => a.Balance);
}
