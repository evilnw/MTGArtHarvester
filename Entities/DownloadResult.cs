using System;

namespace MTGArtHarvester;

public class DownloadResult
{
    public string? FileName { get; set; }

    public string? Folder { get; set; }

    public string? Url { get; set; }

    public double Width { get; set; }
    
    public double Height { get; set; }
}
