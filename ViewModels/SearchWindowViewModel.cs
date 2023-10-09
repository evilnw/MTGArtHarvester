using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTGArtHarvester.Entities;
using MTGArtHarvester.Models.Services;

namespace MTGArtHarvester.ViewModels;

public class SearchWindowViewModel : BaseViewModel
{
    private CancellationTokenSource? _cancellationTokenSource;
    
    private string _searchText = String.Empty;

    private bool _isBusy = false;

    private readonly Func<MtgCard ,Task> _addItemAction;

    private MtgCardViewModel? _selectedItem;

    public ObservableCollection<MtgCardViewModel> SearchResult { get; }

    public bool IsBusy 
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }
    
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); }
    }

    public MtgCardViewModel? SelectedItem
    {
        get => _selectedItem;
        set { _selectedItem = value; OnPropertyChanged(); }
    }
    
    public SearchWindowViewModel(Func<MtgCard, Task> addMtgCardAction)
    {
        SearchResult = new ObservableCollection<MtgCardViewModel>();
        _addItemAction = addMtgCardAction;
    }

    public async Task DoSearch()
    {
        IsBusy = true;
        _cancellationTokenSource?.Cancel();
        SelectedItem = null;
        SearchResult.Clear();
        
        _cancellationTokenSource = new CancellationTokenSource();

        var scryfallService = new ScryfallService();
        var mtgCards = await scryfallService.SearchCardsAsync(_searchText, _cancellationTokenSource.Token);
        
        var mtgCardViewModels = mtgCards.Select(mtgCard => new MtgCardViewModel(mtgCard));
        
        mtgCardViewModels.ToList().ForEach(mtgCardVM => SearchResult.Add(mtgCardVM));
        
        await LoadPreviewImages(_cancellationTokenSource.Token);

        IsBusy = false;
    }

    public async Task DoRandomSearch()
    {
        IsBusy = true;
        _cancellationTokenSource?.Cancel();
        SelectedItem = null;
        SearchResult.Clear();

        _cancellationTokenSource = new CancellationTokenSource();
        
        var scryfallService = new ScryfallService();
        var mtgCards = await scryfallService.RandomCards(@"year>=2007", _cancellationTokenSource.Token, 10);

        var mtgCardViewModels = mtgCards.Select(mtgCard => new MtgCardViewModel(mtgCard));
        
        mtgCardViewModels.ToList().ForEach(mtgCardVM => SearchResult.Add(mtgCardVM));
        
        await LoadPreviewImages(_cancellationTokenSource.Token);

        IsBusy = false;
    }

    public async Task AddToDownloadQueueAsync()
    {
        var selectedItem = SelectedItem;

        if (selectedItem == null)
        {
            return;
        }
        
        if (selectedItem.PreviewImage != null)
        {
            var scryfallService = new ScryfallService();
            scryfallService.AddToImageToCache(selectedItem.SfId, selectedItem.PreviewImage);
        }
        await _addItemAction(selectedItem.MtgCard);
    }

    public async Task AddAllToDownloadQueue()
    {
        var tasks = SearchResult.Select(async mtgCardVM => 
            {
                if (mtgCardVM.PreviewImage != null)
                {
                    var scryfallService = new ScryfallService();
                    scryfallService.AddToImageToCache(mtgCardVM.SfId, mtgCardVM.PreviewImage);
                }
                await _addItemAction(mtgCardVM.MtgCard);
            });
        
        await Task.WhenAll(tasks);
    }

    private async Task LoadPreviewImages(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || !SearchResult.Any())
        {
            return;
        }
        
        var scryfallService = new ScryfallService();

        await Parallel.ForEachAsync<MtgCardViewModel>(
            source: SearchResult, 
            cancellationToken: cancellationToken,
            async (mtgCardVM, cancellationToken) => 
                {
                    mtgCardVM.PreviewImage = await scryfallService.DownloadCropImageAsBitmapAsync(mtgCardVM.SfId, cancellationToken);
                });
    }
}