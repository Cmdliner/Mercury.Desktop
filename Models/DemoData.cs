
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
        new StoreProfile("Adeyemi Stores Ikeja", "Ikeja, Lagos"));

    private static readonly DateTime Now = new(2026, 10, 18, 10, 24, 0);

    public static IReadOnlyList<Transaction> Transactions { get; } =
    [
        new("TXN-92831", "Ikeja", "Sale", 45_000m, TransactionStatus.Completed, "Bank transfer", "Anonymous", Now, "Operating", "Sales revenue"),
        new("TXN-92830", "Yaba", "Sale", 18_500m, TransactionStatus.Completed, "Bank transfer", "Anonymous", Now.AddMinutes(-52), "Operating", "Sales revenue"),
        new("TXN-92829", "Lekki", "Transfer", 100_000m, TransactionStatus.Completed, "Internal", "Lekki branch", Now.AddHours(-3), "Operating", "Settlement"),
        new("TXN-92828", "Ikeja", "Settlement", 250_000m, TransactionStatus.Pending, "Bank transfer", "GTBank", Now.AddHours(-5), "Settlement", "Operating"),
        new("TXN-92827", "Lekki", "Sale", 83_000m, TransactionStatus.Pending, "Bank transfer", "Anonymous", Now.AddHours(-6), "Operating", "Sales revenue"),
        new("TXN-92826", "Ikeja", "Sale", 12_700m, TransactionStatus.Completed, "Bank transfer", "Anonymous", Now.AddHours(-8), "Operating", "Sales revenue"),
        new("TXN-92825", "Yaba", "Transfer", 60_000m, TransactionStatus.Failed, "Internal", "Yaba branch", Now.AddDays(-1), "Operating", "Spending"),
        new("TXN-92824", "Yaba", "Sale", 27_300m, TransactionStatus.Completed, "Bank transfer", "Anonymous", Now.AddDays(-1).AddHours(-2), "Operating", "Sales revenue"),
    ];

    public static IReadOnlyList<StaffMember> Staff { get; } =
    [
        new("Adeola Bakare", "adeola@adeyemistores.ng", "Manager", "Ikeja", "Full"),
        new("Tobi Okafor", "tobi@adeyemistores.ng", "Cashier", "Yaba", "Store"),
        new("Chinedu Eze", "chinedu@adeyemistores.ng", "Cashier", "Lekki", "Store"),
        new("Amaka Obi", "amaka@adeyemistores.ng", "Accountant", "All stores", "Financial"),
    ];

    public static IReadOnlyList<LedgerAccount> Accounts { get; } =
    [
        new("Operating", "Day-to-day payments in and out", 2_410_200m, "Wallet", 0.5),
        new("Settlement", "Waiting to reach your bank", 910_200m, "Transfer", 0.19),
        new("Spending", "Supplies, rent and wages", 1_100_000m, "Store", 0.23),
        new("Reserve", "Set aside for later", 400_000m, "Check", 0.08),
    ];

    public static IReadOnlyList<LedgerEntry> LedgerEntries { get; } =
    [
        new("Oct 18", "Customer payment, Ikeja", "Operating", "Sales revenue", 45_000m),
        new("Oct 18", "Customer payment, Yaba", "Operating", "Sales revenue", 18_500m),
        new("Oct 18", "Branch transfer, Lekki", "Operating", "Settlement", 100_000m),
        new("Oct 17", "Supplier payment", "Spending", "Operating", 320_000m),
        new("Oct 17", "Settlement to bank", "Settlement", "Operating", 250_000m),
    ];

    /// <summary>The signed-in owner in sample mode.</summary>
    public static Services.Api.MeResponse SampleMe { get; } = new(
        Guid.Parse("00000000-0000-0000-0000-000000000001"),
        "Abdulazeez Adeyemi",
        "owner@adeyemistores.ng",
        StaffRole.Owner,
        Guid.Parse("00000000-0000-0000-0000-000000000002"),
        "Adeyemi Stores",
        null,
        "Verified");

    /// <summary>The overview in sample mode, built from the figures above.</summary>
    public static Services.Api.OverviewResponse SampleOverview { get; } = new(
        "Adeyemi Stores",
        3,
        TotalBalance: 4_820_400m,
        AvailableFunds: 3_910_200m,
        PendingSettlement: 910_200m,
        SalesToday: SalesToday,
        SalesLast12Days: SalesBars
            .Select((share, i) => new Services.Api.DailySales(
                DateOnly.FromDateTime(DateTime.Today.AddDays(i - SalesBars.Count + 1)), (decimal)share * 1_300_000m))
            .ToList(),
        RecentTransactions: RecentTransactions
            .Select(t => new Services.Api.TransactionResponse(
                Guid.NewGuid(), "TXN", null, t.Store, "Sale", t.Amount, t.Status, "BankTransfer", null, DateTime.Now))
            .ToList());
}
