using Mercury.Desktop.Models;

namespace Mercury.Desktop.Services.Api;

// The shapes the Mercury API sends and expects. Names follow the server's JSON (camelCase),
// and enums travel as strings. Keep these in step with Mercury.Api/DTOs.

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Email,
    string Password,
    string BusinessName,
    string FirstName,
    string LastName,
    string Phone,
    string? Address);

public record AuthResponse(string Token);

/// <summary>Who is signed in.</summary>
public record MeResponse(
    Guid StaffId,
    string Name,
    string Email,
    StaffRole Role,
    Guid MerchantId,
    string BusinessName,
    Guid? StoreId,
    string Verification);

public record StoreCreateRequest(string Name, string Location);

public record StoreCreateResponse(string Name, Guid StoreId);

public record DailySales(DateOnly Date, decimal Amount);

public record TransactionResponse(
    Guid Id,
    string Reference,
    Guid? StoreId,
    string? StoreName,
    string Type,
    decimal Amount,
    string Status,
    string Method,
    string? Description,
    DateTime CreatedAt);

public record OverviewResponse(
    string BusinessName,
    int StoreCount,
    decimal TotalBalance,
    decimal AvailableFunds,
    decimal PendingSettlement,
    decimal SalesToday,
    IReadOnlyList<DailySales> SalesLast12Days,
    IReadOnlyList<TransactionResponse> RecentTransactions);

public record CollectionAccountResponse(
    Guid Id,
    Guid? StoreId,
    string Kind,
    string? Provider,
    string? BankName,
    string? AccountNumber,
    string? AccountName,
    string Status,
    string? FailureReason);

public record StoreResponse(
    Guid Id,
    string Name,
    string Location,
    decimal? Balance,
    IReadOnlyList<CollectionAccountResponse> CollectionAccounts);

public record StoresListResponse(string MerchantName, IReadOnlyList<StoreResponse> Stores);

/// <summary>Kind is "MercuryVirtual" or "Linked". See the server's CreateCollectionAccountRequest.</summary>
public record CreateCollectionAccountRequest(
    string Kind,
    string? Bvn,
    string? AccountNumber,
    string? BankCode,
    string? PreferredBank,
    string? BankName,
    string? AccountName);

public record TransactionListResponse(IReadOnlyList<TransactionResponse> Items, int Total);

public record TransactionLineResponse(string AccountName, string Direction, decimal Amount);

public record TransactionDetailResponse(TransactionResponse Transaction, IReadOnlyList<TransactionLineResponse> Lines);

public record AccountBalanceResponse(Guid Id, string Code, string Name, Guid? StoreId, decimal Balance, decimal Share);

public record LedgerEntryResponse(
    Guid Id,
    DateTime Date,
    string Reference,
    string? Description,
    string DebitAccount,
    string CreditAccount,
    decimal Amount);

public record AccountsResponse(
    decimal Total,
    IReadOnlyList<AccountBalanceResponse> Accounts,
    IReadOnlyList<LedgerEntryResponse> RecentEntries);

public record StaffResponse(
    Guid Id,
    string Name,
    string Email,
    StaffRole Role,
    Guid? StoreId,
    string? StoreName);

/// <summary>Invites someone by email. A manager or cashier needs a store; an accountant does not.</summary>
public record InviteStaffRequest(string Email, string Name, StaffRole Role, Guid? StoreId);

public record InviteStaffResponse(Guid InvitationId, DateTime ExpiresAt);

public record AcceptInvitationRequest(string Token, string Password);
