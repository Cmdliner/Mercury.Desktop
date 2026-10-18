using Mercury.Desktop.Models;

namespace Mercury.Desktop.Services.Api;

/// <summary>
/// Everything the app asks of Mercury. View models depend on this, not on HTTP, so a screen can
/// be developed and screenshotted against <see cref="DemoMercuryApi"/> without a server.
/// </summary>
public interface IMercuryApi
{
    /// <summary>True for the built-in sample data, false when talking to a real server.</summary>
    bool IsDemo { get; }

    /// <summary>Raised when the server rejects the sign-in token.</summary>
    event EventHandler? Unauthorized;

    /// <summary>Sets (or clears) the token sent with every request.</summary>
    void SetToken(string? token);

    /// <returns>The sign-in token.</returns>
    Task<string> LoginAsync(string email, string password, CancellationToken ct = default);

    /// <summary>Creates the business and its owner, from what onboarding collected.</summary>
    /// <returns>The sign-in token for the new owner.</returns>
    Task<string> RegisterAsync(OnboardingResult account, CancellationToken ct = default);

    /// <returns>The new store, so its payment account can be set up next.</returns>
    Task<StoreCreateResponse> CreateStoreAsync(string name, string location, CancellationToken ct = default);

    /// <summary>
    /// Joins a business from an invitation: the invited person chooses their own password.
    /// </summary>
    /// <returns>The sign-in token for the new member.</returns>
    Task<string> AcceptInvitationAsync(string token, string password, CancellationToken ct = default);

    Task<MeResponse> GetMeAsync(CancellationToken ct = default);

    Task<OverviewResponse> GetOverviewAsync(CancellationToken ct = default);

    /// <summary>Ledger entries as transactions, newest first. A store-scoped person only ever gets their own store.</summary>
    Task<TransactionListResponse> GetTransactionsAsync(
        Guid? storeId, string? type, int page = 1, int pageSize = 50, CancellationToken ct = default);

    /// <summary>One transaction with the debit and credit lines behind it.</summary>
    Task<TransactionDetailResponse> GetTransactionAsync(Guid id, CancellationToken ct = default);

    /// <summary>The accounts that hold money, with their share of the total, and the latest ledger entries.</summary>
    Task<AccountsResponse> GetAccountsAsync(CancellationToken ct = default);

    /// <summary>Everyone the person may see: all staff for an owner, their own store's team for a manager.</summary>
    Task<IReadOnlyList<StaffResponse>> GetStaffAsync(CancellationToken ct = default);

    /// <summary>
    /// Invites someone to join. They are not on the list until they accept and choose a password.
    /// </summary>
    Task<InviteStaffResponse> InviteStaffAsync(InviteStaffRequest request, CancellationToken ct = default);

    /// <summary>The stores the person may see, each with its balance (when allowed) and collection accounts.</summary>
    Task<StoresListResponse> GetStoresAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds somewhere a store receives money. A Mercury account comes back pending until the bank
    /// confirms it; a linked account is active straight away.
    /// </summary>
    Task<CollectionAccountResponse> CreateCollectionAccountAsync(
        Guid storeId, CreateCollectionAccountRequest request, CancellationToken ct = default);
}
