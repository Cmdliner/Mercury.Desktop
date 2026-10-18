using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Services.Api;
using Mercury.Desktop.ViewModels.Workspace;

namespace Mercury.Desktop.ViewModels.Onboarding;

/// <summary>
/// Step 4: where the first store receives money. A Mercury account needs the owner's BVN to open;
/// an account the business already uses just needs recording. It can be skipped and done later
/// from the Payments page.
/// </summary>
public sealed partial class PaymentAccountStepViewModel : OnboardingStepViewModel
{
    public override string ContinueLabel => UseMercuryAccount ? "Create account" : "Add account";

    /// <summary>The business and its store exist by now, so going back would only try to create them twice.</summary>
    public override bool AllowsBack => false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ContinueLabel))]
    private bool _useMercuryAccount = true;

    public MercuryAccountFormViewModel MercuryForm { get; } = new();

    public LinkAccountFormViewModel LinkForm { get; } = new();

    /// <summary>Raised when the owner chooses to set this up later.</summary>
    public event EventHandler? SkipRequested;

    [RelayCommand]
    private void Skip() => SkipRequested?.Invoke(this, EventArgs.Empty);

    /// <returns>The request for the chosen kind of account, or null while its form has errors.</returns>
    public CreateCollectionAccountRequest? TryBuild() =>
        UseMercuryAccount ? MercuryForm.TryBuild() : LinkForm.TryBuild();

    public override bool TryComplete() => TryBuild() is not null;
}
