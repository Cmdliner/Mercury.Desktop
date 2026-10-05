using System.Windows.Input;

namespace Mercury.Desktop;

public class RelayCommand(Action execute, Func<bool>? canExecute = null): ICommand
{
    private readonly Action _execute = execute;
    private readonly Func<bool> _canExecute = canExecute;
    
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    
    public event EventHandler? CanExecuteChanged;
}