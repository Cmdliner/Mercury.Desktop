using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;
using Mercury.Desktop.ViewModels.Login;
using Mercury.Desktop.ViewModels.Onboarding;
using Mercury.Desktop.ViewModels.Workspace;

namespace Mercury.Desktop.ViewModels;

/// <summary>
/// The window's view model: it decides whether to show sign-in, onboarding or the workspace.
/// Sign-in opens the workspace, or onboarding for a new owner; onboarding ends in the workspace;
/// signing out returns to a fresh sign-in screen.
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly OnboardingViewModel _onboarding = new();

    public ShellViewModel()
    {
        _onboarding.Completed += (_, result) => ShowWorkspace(result);
        _login = NewLogin();
        _current = _login;
    }

    /// <summary>The view model of whatever fills the window: sign-in, onboarding or the workspace.</summary>
    [ObservableProperty]
    private object _current;

    private LoginViewModel _login;

    public LoginViewModel Login => _login;

    public OnboardingViewModel Onboarding => _onboarding;

    // TODO: send the onboarding result to the Mercury backend before opening the workspace.
    public void ShowWorkspace(OnboardingResult account)
    {
        var workspace = new WorkspaceViewModel(account);
        workspace.SignedOut += (_, _) => ShowLogin();
        Current = workspace;
    }

    private void ShowLogin()
    {
        _login = NewLogin();
        Current = _login;
    }

    private LoginViewModel NewLogin()
    {
        var login = new LoginViewModel();
        // TODO: the signed-in owner should come from the backend, not the sample account.
        login.SignedIn += (_, _) => ShowWorkspace(DemoData.SampleOnboardingResult);
        login.CreateAccountRequested += (_, _) => Current = _onboarding;
        return login;
    }
}
