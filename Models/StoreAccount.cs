namespace Mercury.Desktop.Models;

/// <summary>A store and the virtual bank account customers pay into for it.</summary>
public sealed record StoreAccount(string Name, string Location, string Bank, string AccountNumber, decimal Balance);
