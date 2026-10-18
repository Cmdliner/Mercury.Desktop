using Mercury.Desktop.Models;

namespace Mercury.Desktop.Models;

/// <summary>One row of the recent-transactions list.</summary>
public sealed record TransactionSummary(string Store, string Status, decimal Amount)
{
    public bool IsPending => Status == "Pending";
}

/// <summary>A figure shown on a stat card: a label, an amount and a short note.</summary>
public sealed record Stat(string Label, decimal Amount, string Note);

/// <summary>
/// Sample figures that mirror the Figma designs. They stand in for the Mercury backend, which
/// will replace them. Keep every made-up number in this file so it is easy to find and remove.
/// </summary>
public static class DemoData
{
    public static readonly IReadOnlyList<Stat> OverviewStats =
    [
        new("Total balance", 4_820_400m, "Across 3 stores"),
        new("Available funds", 3_910_200m, "81% of total"),
        new("Pending settlement", 910_200m, "Updates as received"),
    ];

    public static readonly IReadOnlyList<TransactionSummary> RecentTransactions =
    [
        new("Ikeja", "Completed", 45_000m),
        new("Yaba", "Completed", 18_500m),
        new("Lekki", "Pending", 83_000m),
        new("Ikeja", "Completed", 12_700m),
    ];

    public const decimal SalesToday = 1_240_500m;

    /// <summary>Sales through the day, as a share (0 to 1) of the busiest period.</summary>
    public static readonly IReadOnlyList<double> SalesBars =
        [0.28, 0.40, 0.52, 0.64, 0.82, 0.30, 0.44, 0.56, 0.72, 0.88, 0.34, 0.48];

    public static OnboardingResult SampleOnboardingResult { get; } = new(
        new BusinessProfile("Adeyemi Stores", "owner@adeyemistores.ng", "+234 800 000 0000", "Lagos, Nigeria"),
        new OwnerAccount("Abdulazeez Adeyemi", "owner@adeyemistores.ng", "sample-password"),
        new StoreProfile("Adeyemi Stores Ikeja", "Ikeja, Lagos", "GTBank", "0123456789"));
}
