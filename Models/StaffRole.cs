namespace Mercury.Desktop.Models;

/// <summary>What a person can do in the business. The server decides it; the app only reflects it.</summary>
public enum StaffRole
{
    /// <summary>Runs the whole business and every store.</summary>
    Owner,

    /// <summary>Runs one store and its cashiers.</summary>
    Manager,

    /// <summary>Takes payments for one store.</summary>
    Cashier,

    /// <summary>Reads the books of every store. Cannot change anything.</summary>
    Accountant,
}
