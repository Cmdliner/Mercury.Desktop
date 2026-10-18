namespace Mercury.Desktop.Models;

/// <summary>A bank and the code the payment provider knows it by.</summary>
public sealed record Bank(string Name, string Code);

/// <summary>
/// The banks and wallets offered when adding an account. The codes are the Central Bank's, which
/// Paystack uses; confirm them against the provider's own bank list before relying on a new one.
/// </summary>
public static class NigerianBanks
{
    public static IReadOnlyList<Bank> All { get; } =
    [
        new("Access Bank", "044"),
        new("Fidelity Bank", "070"),
        new("First Bank of Nigeria", "011"),
        new("FCMB", "214"),
        new("Guaranty Trust Bank", "058"),
        new("Kuda Microfinance Bank", "50211"),
        new("Moniepoint Microfinance Bank", "50515"),
        new("OPay", "999992"),
        new("PalmPay", "999991"),
        new("Polaris Bank", "076"),
        new("Stanbic IBTC Bank", "221"),
        new("Sterling Bank", "232"),
        new("Union Bank", "032"),
        new("United Bank for Africa", "033"),
        new("Wema Bank", "035"),
        new("Zenith Bank", "057"),
    ];

    public static IReadOnlyList<string> Names { get; } = All.Select(b => b.Name).ToList();

    public static string? CodeFor(string? name) => All.FirstOrDefault(b => b.Name == name)?.Code;
}
