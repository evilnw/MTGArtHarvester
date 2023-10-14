using System;
using System.Windows.Input;

namespace MTGArtHarvester;

public class Command : ICommand
{
    private readonly Action _execute;
    
    public event EventHandler? CanExecuteChanged;

    public Command(Action execute)
        => _execute = execute;
    
    public bool CanExecute(object? parameter)
        => true; 

    public void Execute(object? parameter = null)
        => _execute.Invoke();
}

public class Command<T> : ICommand
{
    private readonly Action<T> _execute;
    
    public event EventHandler? CanExecuteChanged;

    public Command(Action<T> execute)
        => _execute = execute;

    public bool CanExecute(object? parameter)
        => true;

    public void Execute(object? parameter)
        => _execute.Invoke((T)parameter!);
}
