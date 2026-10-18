namespace Mercury.Desktop.Models;

/// <summary>What the owner told us about the business.</summary>
public sealed record BusinessProfile(string Name, string Email, string Phone, string Address);

/// <summary>The account that will manage the workspace.</summary>
public sealed record OwnerAccount(string FullName, string Email, string Password);

/// <summary>The first store. Where it receives money is set up separately, as a collection account.</summary>
public sealed record StoreProfile(string Name, string Location);

/// <summary>Everything collected by onboarding, ready to send to the Mercury backend.</summary>
public sealed record OnboardingResult(BusinessProfile Business, OwnerAccount Owner, StoreProfile Store);
