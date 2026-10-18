using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Desktop.ViewModels.Onboarding;

public enum StepState
{
    Upcoming,
    Current,
    Complete,
}

/// <summary>One numbered entry in the onboarding sidebar.</summary>
public partial class StepIndicatorViewModel(int number, string title) : ObservableObject
{
    public int Number { get; } = number;

    public string Title { get; } = title;

    [ObservableProperty]
    private StepState _state;
}
