using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Mercury.Desktop.ViewModels;

public class MerchantSetupViewModel: INotifyPropertyChanged
{
    private string _businessName = string.Empty;
    private string _businessEmail = string.Empty;
    private string _businessPhone = string.Empty;
    private string _businessAddress = string.Empty;
    private string _businessType = string.Empty;

    public string BusinessName
    {
        get => _businessName;
        set
        {
            if (_businessName == value) return;
            _businessName = value;
            OnPropertyChanged();
        }
    }

    public string BusinessEmail
    {
        get => _businessEmail;
        set
        {
            if (_businessEmail == value) return;
            _businessEmail = value;
            OnPropertyChanged();
        }
    }

    public string BusinessAddress
    {
        get => _businessAddress;
        set
        {
            if (_businessAddress == value) return;
            _businessAddress = value;
            OnPropertyChanged();
        }
    }

    public string BusinessPhone
    {
        get => _businessPhone;
        set
        {
            if (_businessPhone == value) return;
            _businessPhone = value;
            OnPropertyChanged();
        }
    }

    public string BusinessType
    {
        get => _businessType;
        set
        {
            if (_businessType == value) return;
            _businessType = value;
            OnPropertyChanged();
        }
    }
    
    public ICommand ContinueCommand { get;  }

    public MerchantSetupViewModel()
    {
        ContinueCommand = new RelayCommand(Continue);
    }

    private void Continue()
    {
        // Use this to later validate & continue to next step in onboarding
        
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}