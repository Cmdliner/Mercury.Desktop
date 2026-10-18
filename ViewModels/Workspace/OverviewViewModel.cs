using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.ViewModels.Workspace;

/// <summary>
/// The first page: balances, today's sales and recent activity, read from the server. It shows a
/// loading state while it waits and a retry if the server cannot be reached.
/// </summary>
public sealed partial class OverviewViewModel : PageViewModel
{
    // A bar never shrinks to nothing, so a quiet day still reads as a day.
    private const double MinimumBarShare = 0.04;

    private readonly IMercuryApi _api;

    public OverviewViewModel(string ownerFirstName, IMercuryApi api)
    {
        _api = api;
        Greeting = $"{GreetingFor(DateTime.Now)}, {ownerFirstName}";
        Today = DateTime.Now.ToString("dddd, MMMM d");
    }

    public override string Title => "Overview";

    public string Greeting { get; }

    public string Today { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoaded))]
    private bool _isLoading = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoaded))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => ErrorMessage is not null;

    public bool IsLoaded => !IsLoading && !HasError;

    [ObservableProperty]
    private IReadOnlyList<Stat> _stats = [];

    [ObservableProperty]
    private decimal _salesToday;

    [ObservableProperty]
    private IReadOnlyList<double> _salesBars = [];

    [ObservableProperty]
    private IReadOnlyList<TransactionSummary> _recentTransactions = [];

    [ObservableProperty]
    private bool _hasRecentTransactions;

    /// <summary>Loads the figures. Safe to call again to retry.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Apply(await _api.GetOverviewAsync());
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Apply(OverviewResponse overview)
    {
        var availableShare = overview.TotalBalance == 0 ? 0 : overview.AvailableFunds / overview.TotalBalance;

        Stats =
        [
            new("Total balance", overview.TotalBalance,
                overview.StoreCount == 1 ? "In your store" : $"Across {overview.StoreCount} stores"),
            new("Available funds", overview.AvailableFunds, $"{availableShare:P0} of total"),
            new("Pending settlement", overview.PendingSettlement, "Updates as received"),
        ];

        SalesToday = overview.SalesToday;

        var busiest = overview.SalesLast12Days.Select(d => d.Amount).DefaultIfEmpty(0).Max();
        SalesBars = overview.SalesLast12Days
            .Select(d => busiest == 0 ? MinimumBarShare : Math.Max(MinimumBarShare, (double)(d.Amount / busiest)))
            .ToList();

        RecentTransactions = overview.RecentTransactions
            .Select(t => new TransactionSummary(t.StoreName ?? "Business", t.Status, t.Amount))
            .ToList();
        HasRecentTransactions = RecentTransactions.Count > 0;
    }

    private static string GreetingFor(DateTime now) => now.Hour switch
    {
        < 12 => "Good morning",
        < 17 => "Good afternoon",
        _ => "Good evening",
    };
}
