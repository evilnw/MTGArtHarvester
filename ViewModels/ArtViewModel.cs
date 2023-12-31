﻿using System;
using System.IO;

namespace MTGArtHarvester.ViewModels;

public enum DownloadStatus { InProgress, Completed, NotFound }

public class ArtViewModel : BaseViewModel
{
    private DownloadStatus _status = DownloadStatus.InProgress;
    
    private string? _url;

    private string? _filepath;

    private double _width;
    
    private double _height;
    
    public MtgCardViewModel MtgCardViewModel { get; }

    public DownloadStatus Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public string? Url 
    { 
        get => _url; 
        set { _url = value; OnPropertyChanged(); } 
    }

    public string? FilePath 
    { 
        get => _filepath; 
        set { _filepath = value; OnPropertyChanged(); } 
    }

    public double Width 
    { 
        get => _width; 
        set { _width = value; OnPropertyChanged(); }
    }
    
    public double Height 
    { 
        get => _height; 
        set { _height = value; OnPropertyChanged(); }
    }

    public ArtViewModel(MtgCardViewModel mtgCardViewModel)
        => MtgCardViewModel = mtgCardViewModel;

    public void SetProperties(DownloadResult downloadResult)
    {
        Url = downloadResult.Url;

        if (!String.IsNullOrEmpty(downloadResult.Folder) && !String.IsNullOrEmpty(downloadResult.FileName))
        {
            FilePath = Path.Combine(downloadResult.Folder!, downloadResult.FileName!);
        }

        Status = File.Exists(_filepath) ? DownloadStatus.Completed : DownloadStatus.NotFound;

        Height = downloadResult.Height;
        Width = downloadResult.Width;
    }
}
