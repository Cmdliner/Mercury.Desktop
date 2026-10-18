#if DEBUG
using Mercury.Desktop.Models;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.Dev;

/// <summary>A server that cannot be reached, for checking how the app looks when a request fails.</summary>
internal sealed class OfflineMercuryApi : IMercuryApi
{
    private const string Reason = "Can't reach Mercury right now. Check your connection and try again.";

    public bool IsDemo => false;

    public event EventHandler? Unauthorized
    {
        add { }
        remove { }
    }

    public void SetToken(string? token)
    {
    }

    public Task<string> LoginAsync(string email, string password, CancellationToken ct = default) =>
        Task.FromException<string>(new ApiException(Reason));

    public Task<string> RegisterAsync(OnboardingResult account, CancellationToken ct = default) =>
        Task.FromException<string>(new ApiException(Reason));

    public Task<StoreCreateResponse> CreateStoreAsync(string name, string location, CancellationToken ct = default) =>
        Task.FromException<StoreCreateResponse>(new ApiException(Reason));

    public Task<string> AcceptInvitationAsync(string token, string password, CancellationToken ct = default) =>
        Task.FromException<string>(new ApiException(Reason));

    public Task<MeResponse> GetMeAsync(CancellationToken ct = default) => Task.FromResult(DemoData.SampleMe);

    public Task<OverviewResponse> GetOverviewAsync(CancellationToken ct = default) =>
        Task.FromException<OverviewResponse>(new ApiException(Reason));

    public Task<TransactionListResponse> GetTransactionsAsync(
        Guid? storeId, string? type, int page = 1, int pageSize = 50, CancellationToken ct = default) =>
        Task.FromException<TransactionListResponse>(new ApiException(Reason));

    public Task<TransactionDetailResponse> GetTransactionAsync(Guid id, CancellationToken ct = default) =>
        Task.FromException<TransactionDetailResponse>(new ApiException(Reason));

    public Task<AccountsResponse> GetAccountsAsync(CancellationToken ct = default) =>
        Task.FromException<AccountsResponse>(new ApiException(Reason));

    public Task<IReadOnlyList<StaffResponse>> GetStaffAsync(CancellationToken ct = default) =>
        Task.FromException<IReadOnlyList<StaffResponse>>(new ApiException(Reason));

    public Task<InviteStaffResponse> InviteStaffAsync(InviteStaffRequest request, CancellationToken ct = default) =>
        Task.FromException<InviteStaffResponse>(new ApiException(Reason));

    public Task<StoresListResponse> GetStoresAsync(CancellationToken ct = default) =>
        Task.FromException<StoresListResponse>(new ApiException(Reason));

    public Task<CollectionAccountResponse> CreateCollectionAccountAsync(
        Guid storeId, CreateCollectionAccountRequest request, CancellationToken ct = default) =>
        Task.FromException<CollectionAccountResponse>(new ApiException(Reason));
}
#endif
