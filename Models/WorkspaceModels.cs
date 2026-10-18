namespace Mercury.Desktop.Models;

public enum TransactionStatus
{
    Completed,
    Pending,
    Failed,
}

/// <summary>One payment, transfer or settlement, with the ledger entries it produced.</summary>
public sealed record Transaction(
    string Reference,
    string Store,
    string Type,
    decimal Amount,
    TransactionStatus Status,
    string Method,
    string Customer,
    DateTime Created,
    string DebitAccount,
    string CreditAccount)
{
    public string StatusText => Status.ToString();

    public string CreatedText => Created.ToString("MMM d, yyyy · HH:mm");
}

/// <summary>A person who works in the business and what they are allowed to see.</summary>
public sealed record StaffMember(string Name, string Email, string Role, string Store, string Access)
{
    /// <summary>What this person can do, in plain words, based on their access level.</summary>
    public IReadOnlyList<string> Capabilities => Access switch
    {
        "Full" =>
        [
            "See every store and account they manage",
            "Take payments and record transactions",
            "Add and manage staff",
            "Change store settings",
        ],
        "Financial" =>
        [
            "See the books of every store",
            "View transactions and ledger entries",
            "Cannot change settings or staff",
        ],
        _ =>
        [
            "Take payments for their own store",
            "View that store's transactions",
            "Cannot see other stores or accounts",
        ],
    };

    public string Initials
    {
        get
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..1].ToUpperInvariant(),
                _ => $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant(),
            };
        }
    }
}

/// <summary>A pot of money in the books, such as the operating or settlement account.</summary>
public sealed record LedgerAccount(string Name, string Description, decimal Balance, string IconKey, double Share)
{
    /// <summary>The filled and empty parts of the share bar, as star widths.</summary>
    public System.Windows.GridLength Filled => new(Share, System.Windows.GridUnitType.Star);

    public System.Windows.GridLength Rest => new(1 - Share, System.Windows.GridUnitType.Star);
}

/// <summary>A line in the ledger: money moving from one account to another.</summary>
public sealed record LedgerEntry(string Date, string Description, string From, string To, decimal Amount);
