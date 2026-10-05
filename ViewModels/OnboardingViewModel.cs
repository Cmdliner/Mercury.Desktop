using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Mercury.Desktop.Views;

namespace Mercury.Desktop.ViewModels;

public class OnboardingViewModel: INotifyPropertyChanged
{
    private UserControl _currentView;

    public OnboardingViewModel()
    {
        CurrentView = new MerchantSetupView();
    }
    public UserControl CurrentView
    {
        get => _currentView;
        set => SetField(ref _currentView, value);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}