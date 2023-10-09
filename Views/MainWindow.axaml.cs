using Avalonia.Controls;
using MTGArtHarvester.ViewModels;

namespace MTGArtHarvester.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = new MainWindowViewModel();
        InitializeComponent();
    }
}