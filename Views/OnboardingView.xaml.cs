using System.Windows.Controls;
using Mercury.Desktop.ViewModels;

namespace Mercury.Desktop.Views;

public partial class OnboardingView : UserControl
{
    public OnboardingView()
    {
        InitializeComponent();
        DataContext = new OnboardingViewModel();
    }
}