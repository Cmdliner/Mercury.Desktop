using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercury.Desktop.Models;

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
    private readonly FinishStepViewModel _finish = new();
    private readonly OnboardingStepViewModel[] _steps;

    public OnboardingViewModel()
    {
        _steps = [_business, _owner, _store, _finish];

        Indicators =
        [
            new StepIndicatorViewModel(1, "Business details"),
            new StepIndicatorViewModel(2, "Owner account"),
            new StepIndicatorViewModel(3, "First store"),
            new StepIndicatorViewModel(4, "Finish"),
        ];

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
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    private int _currentIndex;

    public OnboardingStepViewModel CurrentStep => _steps[CurrentIndex];

    public bool ShowsSidebar => CurrentStep.ShowsSidebar;

    public string ContinueLabel => CurrentStep.ContinueLabel;

    public bool CanGoBack => CurrentIndex > 0 && CurrentStep.ShowsSidebar;

    partial void OnCurrentIndexChanged(int value) => RefreshIndicators();

    [RelayCommand]
    private void Continue()
    {
        if (!CurrentStep.TryComplete()) return;
        if (CurrentIndex < _steps.Length - 1) CurrentIndex++;
    }

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
