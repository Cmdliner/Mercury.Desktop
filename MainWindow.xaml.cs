using System.Windows;
using Mercury.Desktop.ViewModels.Onboarding;

namespace Mercury.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var onboarding = new OnboardingViewModel();
        // TODO: send the result to the Mercury backend, then open the dashboard.
        onboarding.Completed += (_, _) => { };
        DataContext = onboarding;
    }
}
