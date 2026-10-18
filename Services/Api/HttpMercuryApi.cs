using Mercury.Desktop.Models;

namespace Mercury.Desktop.Services.Api;

/// <summary>The real thing: every call goes to the Mercury API.</summary>
public sealed class HttpMercuryApi(ApiClient client) : IMercuryApi
{
    public bool IsDemo => false;

    public event EventHandler? Unauthorized
    {
        add => client.Unauthorized += value;
        remove => client.Unauthorized -= value;
    }

    public void SetToken(string? token) => client.SetToken(token);

    public async Task<string> LoginAsync(string email, string password, CancellationToken ct = default) =>
        (await client.PostAsync<AuthResponse>("api/v1/auth/login", new LoginRequest(email.Trim(), password), ct)).Token;

    public async Task<string> RegisterAsync(OnboardingResult account, CancellationToken ct = default)
    {
        // The server wants first and last name apart; onboarding asks for one full name.
        var parts = account.Owner.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstName = parts.FirstOrDefault() ?? account.Owner.FullName;
        var lastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : firstName;

        var request = new RegisterRequest(
            account.Owner.Email.Trim(),
            account.Owner.Password,
            account.Business.Name.Trim(),
            firstName,
            lastName,
            account.Business.Phone.Trim(),
            account.Business.Address.Trim());

        return (await client.PostAsync<AuthResponse>("api/v1/auth/register/merchant", request, ct)).Token;
    }

    public Task<StoreCreateResponse> CreateStoreAsync(string name, string location, CancellationToken ct = default) =>
        client.PostAsync<StoreCreateResponse>("api/v1/merchant/stores", new StoreCreateRequest(name, location), ct);

    public async Task<string> AcceptInvitationAsync(string token, string password, CancellationToken ct = default) =>
        (await client.PostAsync<AuthResponse>("api/v1/auth/invitations/accept", new AcceptInvitationRequest(token, password), ct)).Token;

    public Task<MeResponse> GetMeAsync(CancellationToken ct = default) =>
        client.GetAsync<MeResponse>("api/v1/auth/me", ct);

    public Task<OverviewResponse> GetOverviewAsync(CancellationToken ct = default) =>
        client.GetAsync<OverviewResponse>("api/v1/overview", ct);

    public Task<TransactionListResponse> GetTransactionsAsync(
        Guid? storeId, string? type, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (storeId is { } id) query.Add($"storeId={id}");
        if (!string.IsNullOrEmpty(type)) query.Add($"type={Uri.EscapeDataString(type)}");
        return client.GetAsync<TransactionListResponse>($"api/v1/transactions?{string.Join('&', query)}", ct);
    }

    public Task<TransactionDetailResponse> GetTransactionAsync(Guid id, CancellationToken ct = default) =>
        client.GetAsync<TransactionDetailResponse>($"api/v1/transactions/{id}", ct);

    public Task<AccountsResponse> GetAccountsAsync(CancellationToken ct = default) =>
        client.GetAsync<AccountsResponse>("api/v1/accounts", ct);

    public async Task<IReadOnlyList<StaffResponse>> GetStaffAsync(CancellationToken ct = default) =>
        await client.GetAsync<List<StaffResponse>>("api/v1/staff", ct);

    public Task<InviteStaffResponse> InviteStaffAsync(InviteStaffRequest request, CancellationToken ct = default) =>
        client.PostAsync<InviteStaffResponse>("api/v1/staff/invitations", request, ct);

    public Task<StoresListResponse> GetStoresAsync(CancellationToken ct = default) =>
        client.GetAsync<StoresListResponse>("api/v1/merchant/stores", ct);

    public Task<CollectionAccountResponse> CreateCollectionAccountAsync(
        Guid storeId, CreateCollectionAccountRequest request, CancellationToken ct = default) =>
        client.PostAsync<CollectionAccountResponse>($"api/v1/stores/{storeId}/collection-accounts", request, ct);
}
