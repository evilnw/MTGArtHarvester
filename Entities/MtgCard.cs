using System;

namespace MTGArtHarvester.Entities;

public class MtgCard
{
    /// <summary>
    /// Scryfall card id
    /// </summary>
    public string SfId { get; init; } = String.Empty;
    
    public string Name { get; init; } = String.Empty;
    
    public string SetAbbriviation { get; init; } = String.Empty;

    public string CardNumber { get; init; } = String.Empty;
}
