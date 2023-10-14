using System;
using System.Threading.Tasks;

namespace MTGArtHarvester.Models;

//#pragma warning disable CS0067

public class AsyncCommand<T> : IAsyncCommand<T>
{
    private readonly Func<T,Task> _execute;
    private readonly Func<T, bool>? _canExecute;
    private readonly Action<Exception>? _exceptionAction;

    public event EventHandler? CanExecuteChanged;

    public AsyncCommand(
        Func<T, Task> execute,
        Func<T, bool>? canExecute = null,
        Action<Exception>? exceptionAction = null)
    {
        _execute = execute;
        _canExecute = canExecute;
        _exceptionAction = exceptionAction;
    }

    public async Task ExecuteAsync(T parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        try
        {
            await _execute(parameter);
        }
        catch (Exception ex)
        {
            _exceptionAction?.Invoke(ex);
        }
    }
    
    public bool CanExecute(object? parameter) 
        => _canExecute?.Invoke((T)parameter!) ?? true;

    public void Execute(object? parameter)
        => ExecuteAsync((T)parameter);
}

public class AsyncCommand : IAsyncCommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private readonly Action<Exception>? _exceptionAction;

    public event EventHandler? CanExecuteChanged;

    public AsyncCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null,
        Action<Exception>? exceptionAction = null)
    {
        _execute = execute;
        _canExecute = canExecute;
        _exceptionAction = exceptionAction;
    }

    public async Task ExecuteAsync()
    {
        if (!CanExecute())
        {
            return;
        }

        try
        {
            await _execute();
        }
        catch (Exception ex)
        {
            _exceptionAction?.Invoke(ex);
        }
    }

    public bool CanExecute(object? parameter = null) 
        => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter = null)
        => ExecuteAsync();
}