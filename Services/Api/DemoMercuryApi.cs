using Mercury.Desktop.Models;

namespace Mercury.Desktop.Services.Api;

/// <summary>
/// Sample data with no server behind it. Used by the screenshot tool and the developer switches,
/// so every screen can be reviewed without a backend. It remembers the stores and accounts added
/// during a run, so adding one can be tried and seen.
/// </summary>
public sealed class DemoMercuryApi : IMercuryApi
{
    private readonly List<StoreResponse> _stores = [DemoStore()];

    public bool IsDemo => true;

    public event EventHandler? Unauthorized
    {
        add { }
        remove { }
    }

    public void SetToken(string? token)
    {
    }

    public Task<string> LoginAsync(string email, string password, CancellationToken ct = default) =>
        Task.FromResult("demo-token");

    public Task<string> RegisterAsync(OnboardingResult account, CancellationToken ct = default) =>
        Task.FromResult("demo-token");

    public Task<StoreCreateResponse> CreateStoreAsync(string name, string location, CancellationToken ct = default)
    {
        var id = Guid.NewGuid();
        _stores.Add(new StoreResponse(id, name, location, 0m, []));
        return Task.FromResult(new StoreCreateResponse(name, id));
    }

    public Task<string> AcceptInvitationAsync(string token, string password, CancellationToken ct = default) =>
        Task.FromResult("demo-token");

    public Task<MeResponse> GetMeAsync(CancellationToken ct = default) => Task.FromResult(DemoData.SampleMe);

    public Task<OverviewResponse> GetOverviewAsync(CancellationToken ct = default) =>
        Task.FromResult(DemoData.SampleOverview);

    public Task<StoresListResponse> GetStoresAsync(CancellationToken ct = default) =>
        Task.FromResult(new StoresListResponse(DemoData.SampleMe.BusinessName, _stores.ToList()));

    public Task<CollectionAccountResponse> CreateCollectionAccountAsync(
        Guid storeId, CreateCollectionAccountRequest request, CancellationToken ct = default)
    {
        var linked = request.Kind == "Linked";
        var account = new CollectionAccountResponse(
            Guid.NewGuid(), storeId, request.Kind, linked ? null : "Paystack",
            request.BankName, request.AccountNumber, request.AccountName ?? "ADEYEMI STORES",
            linked ? "Active" : "Pending", null);

        var index = _stores.FindIndex(s => s.Id == storeId);
        if (index >= 0)
        {
            var store = _stores[index];
            _stores[index] = store with { CollectionAccounts = [.. store.CollectionAccounts, account] };
        }

        return Task.FromResult(account);
    }

    public Task<TransactionListResponse> GetTransactionsAsync(
        Guid? storeId, string? type, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var rows = DemoData.Transactions
            .Where(t => string.IsNullOrEmpty(type) || t.Type == type)
            .Where(t => storeId is null || t.Store == DemoStoreName(storeId.Value))
            .OrderByDescending(t => t.Created)
            .Select(ToResponse)
            .ToList();
        return Task.FromResult(new TransactionListResponse(rows, rows.Count));
    }

    public Task<TransactionDetailResponse> GetTransactionAsync(Guid id, CancellationToken ct = default)
    {
        var t = DemoData.Transactions.First(t => IdFor(t) == id);
        return Task.FromResult(new TransactionDetailResponse(ToResponse(t),
        [
            new TransactionLineResponse(t.DebitAccount, "Debit", t.Amount),
            new TransactionLineResponse(t.CreditAccount, "Credit", t.Amount),
        ]));
    }

    public Task<AccountsResponse> GetAccountsAsync(CancellationToken ct = default) =>
        Task.FromResult(new AccountsResponse(
            DemoData.Accounts.Sum(a => a.Balance),
            DemoData.Accounts
                .Select(a => new AccountBalanceResponse(
                    Guid.NewGuid(), DemoCode(a.Name), a.Name, null, a.Balance, (decimal)a.Share))
                .ToList(),
            DemoData.LedgerEntries
                .Select(e => new LedgerEntryResponse(
                    Guid.NewGuid(), DateTime.Today, "REF", e.Description, e.From, e.To, e.Amount))
                .ToList()));

    public Task<IReadOnlyList<StaffResponse>> GetStaffAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<StaffResponse>>(DemoData.Staff
            .Select(p => new StaffResponse(
                Guid.NewGuid(), p.Name, p.Email, Enum.Parse<StaffRole>(p.Role), null,
                p.Store == "All stores" ? null : p.Store))
            .ToList());

    public Task<InviteStaffResponse> InviteStaffAsync(InviteStaffRequest request, CancellationToken ct = default) =>
        Task.FromResult(new InviteStaffResponse(Guid.NewGuid(), DateTime.UtcNow.AddDays(7)));

    // Demo transactions have no ids of their own, so one is made from the reference.
    private static Guid IdFor(Transaction t) => new(System.Security.Cryptography.MD5.HashData(
        System.Text.Encoding.UTF8.GetBytes(t.Reference)));

    // The app words and decorates an account by the end of its code, as it does for real ones.
    private static string DemoCode(string name) => name switch
    {
        "Operating" => "DEMO-OPERATING",
        "Settlement" => "DEMO-CLEARING",
        "Reserve" => "DEMO-RESERVE",
        _ => "DEMO-CASH-TILL",
    };

    private static string DemoStoreName(Guid id) => DemoData.SampleOnboardingResult.Store.Name;

    private static TransactionResponse ToResponse(Transaction t) => new(
        IdFor(t), t.Reference, null, t.Store, t.Type, t.Amount, t.StatusText, t.Method == "Bank transfer" ? "BankTransfer" : t.Method, t.Customer, t.Created);

    private static StoreResponse DemoStore()
    {
        var sample = DemoData.SampleOnboardingResult.Store;
        var storeId = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
        return new StoreResponse(
            storeId, sample.Name, sample.Location, 1_820_400m,
            [
                new CollectionAccountResponse(
                    Guid.NewGuid(), storeId, "MercuryVirtual", "Paystack", "Guaranty Trust Bank",
                    "0123456789", "ADEYEMI STORES", "Active", null),
            ]);
    }
}
