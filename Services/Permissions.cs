using Mercury.Desktop.Models;

namespace Mercury.Desktop.Services;

/// <summary>
/// What each role sees and may do on screen. This only hides what a person cannot use: the
/// server enforces the same rules, so hiding a button is never the only protection.
/// </summary>
public static class Permissions
{
    public const string Overview = "Overview";
    public const string Stores = "Stores";
    public const string Payments = "Payments";
    public const string Transactions = "Transactions";
    public const string Accounts = "Accounts";
    public const string Staff = "Staff";

    public static bool CanSee(StaffRole role, string page) => role switch
    {
        StaffRole.Owner or StaffRole.Manager => true,
        StaffRole.Accountant => page is Overview or Stores or Transactions or Accounts,
        StaffRole.Cashier => page is Overview or Stores or Payments or Transactions,
        _ => false,
    };

    public static bool CanCreateStores(StaffRole role) => role == StaffRole.Owner;

    public static bool CanInviteStaff(StaffRole role) => role is StaffRole.Owner or StaffRole.Manager;
}
