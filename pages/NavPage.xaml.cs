using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.ApplicationSettings;

namespace sap_gui.Pages;

public sealed partial class NavPage : Page
{
    public ObservableCollection<TileItem> TileItems { get; } = new();

    public NavPage()
    {
        InitializeComponent();
        LoadTiles();
    }

    private void LoadTiles()
    {
        TileItems.Add(new TileItem { Title = "Inventory", IconPath = "ms-appx:///Assets/staging.png" });
        TileItems.Add(new TileItem { Title = "Transfer", IconPath = "ms-appx:///Assets/transfer.png" });
        TileItems.Add(new TileItem { Title = "Picksheet", IconPath = "ms-appx:///Assets/picklist.png" });
    }

    private void TileButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.DataContext is not TileItem tile) return;

        switch (tile.Title)
        {

            case "Inventory":
                Frame.Navigate(typeof(StockPage));
                break;

            case "Transfer":
                Frame.Navigate(typeof(TransferPage));
                break;

            case "Picksheet":
                Frame.Navigate(typeof(PicksheetPage));
                break;

            default:
                // If needed: log unknown tile
                break;
        }
    }
}

public class TileItem
{
    public string Title { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
}
