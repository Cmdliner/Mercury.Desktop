using CommunityToolkit.Mvvm.Input;

namespace Mercury.Desktop.ViewModels.Onboarding;

/// <summary>Step 4: the "Mercury is ready" screen. It has no sidebar and no Continue button.</summary>
public partial class FinishStepViewModel : OnboardingStepViewModel
{
    public override bool ShowsSidebar => false;

    /// <summary>Raised when the owner chooses to go to the dashboard.</summary>
    public event EventHandler? DashboardRequested;

    [RelayCommand]
    private void GoToDashboard() => DashboardRequested?.Invoke(this, EventArgs.Empty);
}
