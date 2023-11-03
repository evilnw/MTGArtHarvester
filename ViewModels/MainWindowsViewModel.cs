using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Windows.Input;
using MTGArtHarvester.Models;
using MTGArtHarvester.Views;
using MTGArtHarvester.Models.Commands;

namespace MTGArtHarvester.ViewModels;

public partial class MainWindowViewModel : BaseViewModel
{
    public ArtDownloadViewModel ArtDownloadViewModel { get; }

    public ICommand OpenSearchCommand { get; }

    public IAsyncCommand PickDestinationFolderCommand { get; }

    public ICommand OpenDestinationFolderCommand { get; }

    public IAsyncCommand<string> ClearItemsCommand { get; }
    
    public MainWindowViewModel()
    { 
        ArtDownloadViewModel = new ArtDownloadViewModel();
        OpenSearchCommand = new Command(OpenSearchWindow);
        PickDestinationFolderCommand = new AsyncCommand(PickDestinationFolder);
        OpenDestinationFolderCommand = new Command(OpenDestinationFolder);
        ClearItemsCommand = new AsyncCommand<string>(ClearItems);
    }

    private void OpenSearchWindow()
    {
        var searchWindow = new SearchWindow()
        {
            DataContext = new SearchWindowViewModel(ArtDownloadViewModel.AddToDownloadQueueAsync)
        };
        searchWindow.Show();
    }

    private async Task PickDestinationFolder()
    {
        var mainWindow = ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow;
        var storageProvider = TopLevel.GetTopLevel(mainWindow)!.StorageProvider;
        
        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        
        if (!folders.Any())
        {
            return;
        }

        ArtDownloadViewModel.DestinationFolder = folders[0].TryGetLocalPath();
    }

    private void OpenDestinationFolder()
    {
        if (String.IsNullOrEmpty(ArtDownloadViewModel.DestinationFolder))
        {
            return;
        }
        
        new Process
        {
            StartInfo = new ProcessStartInfo(ArtDownloadViewModel.DestinationFolder)
            {
                UseShellExecute = true
            }
        }.Start();
    }

    private async Task ClearItems(string parameters)
    {
        if (parameters.StartsWith("Status:NotFound", StringComparison.OrdinalIgnoreCase))
        {
            await ArtDownloadViewModel.RemoveItems((artVM) => { return artVM.Status == DownloadStatus.NotFound; });
        }
        else if (parameters.StartsWith("Width:"))
        {
            await ArtDownloadViewModel.RemoveItems((artVM) => 
                { 
                    return artVM.Width < int.Parse(parameters.Replace("Width:", "")) && artVM.Status == DownloadStatus.Completed;
                });
        }
        else if (parameters.StartsWith("Height:"))
        {
            await ArtDownloadViewModel.RemoveItems((artVM) => 
                { 
                    return artVM.Height < int.Parse(parameters.Replace("Height:", "")) && artVM.Status == DownloadStatus.Completed;
                });
        }
    }
}