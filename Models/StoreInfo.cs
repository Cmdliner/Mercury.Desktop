namespace Mercury.Desktop.Models;

/// <summary>Somewhere a store receives money: a Mercury virtual account or a bank account the business already uses.</summary>
public sealed record CollectionAccountInfo(
    Guid Id,
    string Kind,
    string Status,
    string? BankName,
    string? AccountNumber,
    string? AccountName,
    string? FailureReason)
{
    public bool IsActive => Status == "Active";

    public bool IsPending => Status == "Pending";

    public bool IsFailed => Status == "Failed";

    public bool IsMercury => Kind == "MercuryVirtual";

    public string KindLabel => IsMercury ? "Mercury account" : "Linked account";

    public string StatusLabel => Status switch
    {
        "Active" => "Active",
        "Pending" => "Setting up",
        "Failed" => "Needs attention",
        _ => Status,
    };

    /// <summary>One line for a list: the bank and number.</summary>
    public string Summary => $"{BankName ?? "Bank"} {AccountNumber}".Trim();
}

/// <summary>A store as the stores and payments pages show it.</summary>
public sealed record StoreInfo(
    Guid Id,
    string Name,
    string Location,
    decimal? Balance,
    IReadOnlyList<CollectionAccountInfo> Accounts)
{
    /// <summary>The account to give a customer: the first active one, else whatever there is.</summary>
    public CollectionAccountInfo? Primary => Accounts.FirstOrDefault(a => a.IsActive) ?? Accounts.FirstOrDefault();

    /// <summary>The other accounts, shown smaller beside the main one.</summary>
    public IReadOnlyList<CollectionAccountInfo> OtherAccounts => Accounts.Where(a => a != Primary).ToList();

    public bool HasOtherAccounts => OtherAccounts.Count > 0;

    public bool HasBalance => Balance.HasValue;

    public decimal BalanceValue => Balance ?? 0m;

    public bool HasAccount => Accounts.Count > 0;

    public bool HasNoAccount => !HasAccount;

    public bool IsActive => Primary?.IsActive == true;

    public bool IsNotActive => !IsActive;

    public bool IsPending => Primary?.IsPending == true;

    public bool IsFailed => Primary?.IsFailed == true;

    public string StatusLabel => Primary switch
    {
        null => "No account yet",
        { IsActive: true } => "Active",
        { IsPending: true } => "Setting up",
        _ => "Needs attention",
    };

    /// <summary>The bank line on a store card.</summary>
    public string BankLine => Primary?.BankName ?? "No payment account yet";

    public string NumberLine => Primary?.AccountNumber ?? string.Empty;
}
