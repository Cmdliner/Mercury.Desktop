using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mercury.Desktop.ViewModels;

public class OwnerAccountSetupViewModel : INotifyPropertyChanged
{
    private string _fullName  = "John Doe";
    private string _emailAddress = "owner@mercury.co";
    private string _password = "TestPassword123";
    private string _confirmPassword  = "TestPassword123";


    public string FullName
    {
        get => _fullName;
        set
        {
            if (_fullName == value) return;
            _fullName = value;
            OnPropertyChanged();
        }
    }

    public string EmailAddress
    {
        get => _emailAddress;
        set
        {
            if (_emailAddress == value) return;
            _emailAddress = value;
            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password == value) return;
            _password = value;
            OnPropertyChanged();
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (_confirmPassword == value) return;
            _confirmPassword = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}