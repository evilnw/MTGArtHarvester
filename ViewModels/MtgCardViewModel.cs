using Avalonia.Media.Imaging;
using MTGArtHarvester.Entities;

namespace MTGArtHarvester.ViewModels;

public class MtgCardViewModel : BaseViewModel
{
    private Bitmap? _previewImage;

    public readonly MtgCard MtgCard;

    public string Name => MtgCard.Name;

    public string SetAbbriviation => MtgCard.SetAbbriviation;

    public string CardNumber => MtgCard.CardNumber;

    public string SfId => MtgCard.SfId;

    public Bitmap? PreviewImage
    {
        get => _previewImage;
        set { _previewImage = value; OnPropertyChanged(); }
    }
    
    public MtgCardViewModel(MtgCard mtgCard)
        => MtgCard = mtgCard;
}