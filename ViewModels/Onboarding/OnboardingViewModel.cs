using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;
using Mercury.Desktop.Services.Api;

namespace Mercury.Desktop.ViewModels.Onboarding;

/// <summary>
/// Runs the four onboarding steps: business details, owner account, first store, finish.
/// It owns the step view models and decides which one is current. It knows nothing about
/// views: the matching view for each step is chosen in OnboardingView.xaml.
/// </summary>
public partial class OnboardingViewModel : ObservableObject
{
    private readonly BusinessDetailsStepViewModel _business = new();
    private readonly OwnerAccountStepViewModel _owner = new();
    private readonly FirstStoreStepViewModel _store = new();
    private readonly PaymentAccountStepViewModel _payment = new();
    private readonly FinishStepViewModel _finish = new();
    private readonly OnboardingStepViewModel[] _steps;

    public OnboardingViewModel()
    {
        _steps = [_business, _owner, _store, _payment, _finish];

        Indicators =
        [
            new StepIndicatorViewModel(1, "Business details"),
            new StepIndicatorViewModel(2, "Owner account"),
            new StepIndicatorViewModel(3, "First store"),
            new StepIndicatorViewModel(4, "Payment account"),
            new StepIndicatorViewModel(5, "Finish"),
        ];

        // The payment step's button text depends on which kind of account is chosen.
        _payment.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(ContinueLabel)) return;
            OnPropertyChanged(nameof(ContinueLabel));
            OnPropertyChanged(nameof(ContinueText));
        };
        _payment.SkipRequested += (_, _) => Advance();

        _finish.DashboardRequested += (_, _) => Completed?.Invoke(this, BuildResult());
        RefreshIndicators();
    }

    /// <summary>Raised when the owner leaves onboarding for the dashboard.</summary>
    public event EventHandler<OnboardingResult>? Completed;

    public IReadOnlyList<StepIndicatorViewModel> Indicators { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentStep))]
    [NotifyPropertyChangedFor(nameof(ShowsSidebar))]
    [NotifyPropertyChangedFor(nameof(ContinueLabel))]
    [NotifyPropertyChangedFor(nameof(ContinueText))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    private int _currentIndex;

    public OnboardingStepViewModel CurrentStep => _steps[CurrentIndex];

    public bool ShowsSidebar => CurrentStep.ShowsSidebar;

    public string ContinueLabel => CurrentStep.ContinueLabel;

    public bool CanGoBack => CurrentIndex > 0 && CurrentStep.ShowsSidebar && CurrentStep.AllowsBack;

    partial void OnCurrentIndexChanged(int value) => RefreshIndicators();

    /// <summary>
    /// Creates the business from what was collected. Returns an error message to show, or null on
    /// success. Set by whoever owns the server connection; null means there is nothing to submit.
    /// </summary>
    public Func<OnboardingResult, Task<string?>>? Submit { get; set; }

    /// <summary>Asks for the first store's payment account. Returns an error message to show, or null on success.</summary>
    public Func<CreateCollectionAccountRequest, Task<string?>>? SubmitAccount { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    [NotifyPropertyChangedFor(nameof(ContinueText))]
    private bool _isBusy;

    /// <summary>Why creating the business failed, shown next to Continue.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string ContinueText => IsBusy ? "Creating your workspace..." : ContinueLabel;

    [RelayCommand(CanExecute = nameof(CanContinue))]
    private async Task ContinueAsync()
    {
        ErrorMessage = null;
        if (!CurrentStep.TryComplete()) return;

        // The last form step creates the business, so the "ready" screen is only shown once it exists.
        if (CurrentStep == _store && Submit is not null)
        {
            IsBusy = true;
            try
            {
                var error = await Submit(BuildResult());
                if (error is not null)
                {
                    ErrorMessage = error;
                    return;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // The payment step asks for the account once the store exists.
        if (CurrentStep == _payment && SubmitAccount is not null && _payment.TryBuild() is { } request)
        {
            IsBusy = true;
            try
            {
                var error = await SubmitAccount(request);
                if (error is not null)
                {
                    ErrorMessage = error;
                    return;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        Advance();
    }

    private void Advance()
    {
        ErrorMessage = null;
        if (CurrentIndex < _steps.Length - 1) CurrentIndex++;
    }

    private bool CanContinue() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void Back() => CurrentIndex--;

    /// <summary>Jumps straight to a step. Used by the screenshot tool and tests.</summary>
    public void GoTo(int index) => CurrentIndex = Math.Clamp(index, 0, _steps.Length - 1);

    private void RefreshIndicators()
    {
        for (var i = 0; i < Indicators.Count; i++)
        {
            Indicators[i].State = i < CurrentIndex ? StepState.Complete
                : i == CurrentIndex ? StepState.Current
                : StepState.Upcoming;
        }
    }

    private OnboardingResult BuildResult() =>
        new(_business.ToProfile(), _owner.ToAccount(), _store.ToProfile());
}
