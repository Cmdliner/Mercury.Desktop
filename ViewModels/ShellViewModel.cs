using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services;
using Mercury.Desktop.Services.Api;
using Mercury.Desktop.ViewModels.Login;
using Mercury.Desktop.ViewModels.Onboarding;
using Mercury.Desktop.ViewModels.Workspace;

namespace Mercury.Desktop.ViewModels;

/// <summary>
/// The window's view model: it decides whether to show the startup screen, sign-in, onboarding or
/// the workspace, and keeps the sign-in token. Sign-in opens the workspace, or onboarding for a
/// new owner; onboarding ends in the workspace; signing out, or a token the server no longer
/// accepts, returns to a fresh sign-in screen.
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IMercuryApi _api;
    private readonly TokenStore? _tokens;
    private readonly OnboardingViewModel _onboarding = new();

    // Set once the business exists but before the person has pressed "Go to dashboard",
    // so a retry after a failed store creation does not register the business twice.
    private bool _businessRegistered;

    // The first store, kept so the payment-account step knows which store it is for.
    private Guid _firstStoreId;

    public ShellViewModel(IMercuryApi api, TokenStore? tokens = null)
    {
        _api = api;
        _tokens = tokens;

        _onboarding.Submit = SubmitOnboardingAsync;
        _onboarding.SubmitAccount = SubmitFirstAccountAsync;
        _onboarding.Completed += async (_, _) => await OpenWorkspaceAsync();
        _api.Unauthorized += (_, _) =>
            Application.Current?.Dispatcher.Invoke(() => SignOut("Your session expired. Please sign in again."));

        _login = NewLogin();
        _current = tokens?.Load() is not null ? new SplashViewModel() : _login;

        // Opened from an invitation link: go straight to joining.
        if (AppServices.PendingInvitationToken is { } invitation)
        {
            AppServices.PendingInvitationToken = null;
            ShowAcceptInvitation(invitation);
        }
    }

    /// <summary>The view model of whatever fills the window.</summary>
    [ObservableProperty]
    private object _current;

    private LoginViewModel _login;

    public LoginViewModel Login => _login;

    public OnboardingViewModel Onboarding => _onboarding;

    /// <summary>Picks up where the person left off: a saved sign-in goes straight to the workspace.</summary>
    public async Task InitializeAsync()
    {
        // An invitation link takes priority over any saved sign-in.
        if (Current is AcceptInvitationViewModel) return;

        var token = _tokens?.Load();
        if (token is null) return;

        _api.SetToken(token);
        try
        {
            ShowWorkspace(await _api.GetMeAsync());
        }
        catch (ApiException ex)
        {
            // An expired token was already cleared by SignOut. Any other problem (no network, say)
            // keeps the saved sign-in and just shows the reason on the sign-in screen.
            if (Current is SplashViewModel) ShowLogin(ex.Message);
        }
    }

    public void ShowWorkspace(MeResponse me)
    {
        var workspace = new WorkspaceViewModel(me, _api);
        workspace.SignedOut += (_, _) => SignOut(null);
        Current = workspace;
    }

    /// <summary>The screen where an invited person chooses a password. The token is known when it came from a link.</summary>
    public void ShowAcceptInvitation(string? token)
    {
        var accept = new AcceptInvitationViewModel(_api, token);
        accept.Accepted += async (_, newToken) => await OnSignedInAsync(newToken);
        accept.BackRequested += (_, _) => ShowLogin(null);
        Current = accept;
    }

    private async Task OpenWorkspaceAsync()
    {
        try
        {
            ShowWorkspace(await _api.GetMeAsync());
        }
        catch (ApiException ex)
        {
            ShowLogin(ex.Message);
        }
    }

    private async Task OnSignedInAsync(string token)
    {
        _api.SetToken(token);
        _tokens?.Save(token);
        await OpenWorkspaceAsync();
    }

    /// <summary>Creates the business and its first store. Returns a message to show if it failed.</summary>
    private async Task<string?> SubmitOnboardingAsync(OnboardingResult result)
    {
        try
        {
            if (!_businessRegistered)
            {
                var token = await _api.RegisterAsync(result);
                _api.SetToken(token);
                _tokens?.Save(token);
                _businessRegistered = true;
            }

            _firstStoreId = (await _api.CreateStoreAsync(result.Store.Name, result.Store.Location)).StoreId;
            _businessRegistered = false;
            return null;
        }
        catch (ApiException ex)
        {
            return ex.Message;
        }
    }

    /// <summary>Asks for the first store's payment account. Returns a message to show if it failed.</summary>
    private async Task<string?> SubmitFirstAccountAsync(CreateCollectionAccountRequest request)
    {
        try
        {
            await _api.CreateCollectionAccountAsync(_firstStoreId, request);
            return null;
        }
        catch (ApiException ex)
        {
            return ex.Message;
        }
    }

    private void SignOut(string? message)
    {
        _tokens?.Clear();
        _api.SetToken(null);
        ShowLogin(message);
    }

    private void ShowLogin(string? message)
    {
        _login = NewLogin();
        _login.Message = message;
        Current = _login;
    }

    private LoginViewModel NewLogin()
    {
        var login = new LoginViewModel(_api);
        login.SignedIn += async (_, token) => await OnSignedInAsync(token);
        login.CreateAccountRequested += (_, _) => Current = _onboarding;
        login.InvitationRequested += (_, _) => ShowAcceptInvitation(null);
        return login;
    }
}
