using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;
using Mercury.Desktop.ViewModels.Login;
using Mercury.Desktop.ViewModels.Onboarding;
using Mercury.Desktop.ViewModels.Workspace;

namespace Mercury.Desktop.ViewModels;

/// <summary>
/// The window's view model: it decides whether to show sign-in, onboarding or the workspace.
/// Sign-in opens the workspace, or onboarding for a new owner; onboarding ends in the workspace.
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly LoginViewModel _login = new();
    private readonly OnboardingViewModel _onboarding = new();

    public ShellViewModel()
    {
        // TODO: the signed-in owner should come from the backend, not the sample account.
        _login.SignedIn += (_, _) => ShowWorkspace(DemoData.SampleOnboardingResult);
        _login.CreateAccountRequested += (_, _) => Current = _onboarding;
        _onboarding.Completed += (_, result) => ShowWorkspace(result);
        _current = _login;
    }

    /// <summary>The view model of whatever fills the window: sign-in, onboarding or the workspace.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDarkCaptionArea))]
    private object _current;

    /// <summary>True when the top right of the window is dark, so the caption buttons must be light.</summary>
    public bool HasDarkCaptionArea => Current is LoginViewModel;

    public LoginViewModel Login => _login;

    public OnboardingViewModel Onboarding => _onboarding;

    // TODO: send the onboarding result to the Mercury backend before opening the workspace.
    public void ShowWorkspace(OnboardingResult account) => Current = new WorkspaceViewModel(account);
}
