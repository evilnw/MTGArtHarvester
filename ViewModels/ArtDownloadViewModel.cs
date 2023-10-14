using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTGArtHarvester.Entities;
using MTGArtHarvester.Models.Services;

namespace MTGArtHarvester.ViewModels;

public class ArtDownloadViewModel : BaseViewModel
{
    private string? _destanationFolder;

    private readonly SemaphoreSlim _semaphoreSlim;
        
    public ObservableCollection<ArtViewModel> Items { get; }

    public string? DestinationFolder 
    {
        get => _destanationFolder;
        set { _destanationFolder = value; OnPropertyChanged(); }
    }

    public ArtDownloadViewModel()
    { 
        _semaphoreSlim = new SemaphoreSlim(1);
        Items = new ObservableCollection<ArtViewModel>();
        DestinationFolder = GetDefaultDestinationFolder(); 
    }

    public async Task AddToDownloadQueueAsync(MtgCard mtgCard)
    {
        await _semaphoreSlim.WaitAsync();
        
        if (Items.Any(art => art.MtgCardViewModel.SfId == mtgCard.SfId))
        {
            return;
        }
        
        var artViewModel = new ArtViewModel(new MtgCardViewModel(mtgCard));
        var mtgCardViewModel = new MtgCardViewModel(mtgCard);
        Items.Add(artViewModel);
        
        _semaphoreSlim.Release();
        
        var scryfallService = new ScryfallService();
        artViewModel.MtgCardViewModel.PreviewImage = await scryfallService.DownloadCropImageAsBitmapAsync(mtgCardViewModel.SfId);

        var artDownloadService = new ArtDownloadService();
        var downloadResult = await artDownloadService.DownloadArtAsync(mtgCard, DestinationFolder!);
        artViewModel.SetProperties(downloadResult);
    }

    public async Task RemoveNotFoundItems()
    {
        await _semaphoreSlim.WaitAsync();

        Items.Where(artViewModel => artViewModel.Status == "NotFound")
            .ToList()
            .ForEach(artViewModel => Items.Remove(artViewModel));

        _semaphoreSlim.Release();
    }

    private string GetDefaultDestinationFolder()
    {
        var downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        
        if (Directory.Exists(downloadsFolder))
        {
            return downloadsFolder;
        }
                
        if (!Directory.Exists("./Images"))
        {
            Directory.CreateDirectory("./Images");
        }
        return Path.Combine(Directory.GetCurrentDirectory(), "Images");
    }
}
