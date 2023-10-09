using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;
using MTGArtHarvester.Views;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Diagnostics;
using System;

namespace MTGArtHarvester.ViewModels;

public partial class MainWindowViewModel : BaseViewModel
{
    public ArtDownloadViewModel ArtDownloadViewModel { get; }
    
    public MainWindowViewModel()
    { 
        ArtDownloadViewModel = new ArtDownloadViewModel();
    }

    public void OpenSearchWindow()
    {
        var searchWindow = new SearchWindow()
        {
            DataContext = new SearchWindowViewModel(ArtDownloadViewModel.AddToDownloadQueueAsync)
        };
        searchWindow.Show();
    }

    public async Task PickDestinationFolder()
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

    public void OpenDestinationFolder()
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
}