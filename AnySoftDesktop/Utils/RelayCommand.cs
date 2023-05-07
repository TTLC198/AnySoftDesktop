using System;
using System.Windows.Input;

namespace AnySoftDesktop.Utils;

public class RelayCommand : ICommand
{
    private Action<object> execute;
    private Func<object, bool>? canExecute;
 
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
 
    public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }
 
    public bool CanExecute(object? parameter)
    {
        if (canExecute is null || parameter is null)
            return false;
        return canExecute(parameter);
    }
 
    public void Execute(object? parameter)
    {
        if (parameter is null)
            return;
        execute(parameter);
    }
}