using Mercury.Desktop.Models;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>The Overview page: balances, today's sales and reconciliation at a glance.</summary>
public sealed class OverviewViewModel(string ownerFirstName) : PageViewModel
{
    public override string Title => "Overview";

    /// <summary>For example "Good afternoon, Abdulazeez".</summary>
    public string Greeting { get; } = $"{GreetingFor(DateTime.Now)}, {ownerFirstName}";

    /// <summary>For example "Wednesday, September 30".</summary>
    public string Today { get; } = DateTime.Now.ToString("dddd, MMMM d");

    public IReadOnlyList<Stat> Stats { get; } = DemoData.OverviewStats;

    public decimal SalesToday { get; } = DemoData.SalesToday;

    public IReadOnlyList<double> SalesBars { get; } = DemoData.SalesBars;

    public IReadOnlyList<TransactionSummary> RecentTransactions { get; } = DemoData.RecentTransactions;

    /// <summary>Payments received so far today, shown on the reconciliation card.</summary>
    public decimal ReconciledAmount { get; } = DemoData.SalesToday;

    public bool IsReconciled { get; } = true;

    private static string GreetingFor(DateTime now) => now.Hour switch
    {
        < 12 => "Good morning",
        < 17 => "Good afternoon",
        _ => "Good evening",
    };
}
