using System.Threading.Tasks;
using System.Windows.Input;

namespace MTGArtHarvester.Models;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync();
}

public interface IAsyncCommand<in T> : ICommand
{
    Task ExecuteAsync(T parameter);
}
