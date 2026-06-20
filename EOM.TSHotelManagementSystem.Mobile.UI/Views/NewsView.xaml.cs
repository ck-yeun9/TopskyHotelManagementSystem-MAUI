
namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class NewsView : ContentView
{
    private readonly NewsViewModel _viewModel;
    public NewsView(NewsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = _viewModel = viewModel;

        if (NewsCollection != null)
        {
            NewsCollection.Scrolled += OnNewsCollectionScrolled;
        }
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null && BindingContext is NewsViewModel vm)
        {
            _ = vm.LoadNewsAsync();
        }
    }

    private async void OnNewsCollectionScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (BindingContext is not NewsViewModel viewModel)
            return;

        if (viewModel.IsRefreshing || viewModel.IsLoadingMore)
            return;

        var totalItems = viewModel.NewsItems.Count;
        if (totalItems == 0) return;

        if (e.LastVisibleItemIndex >= totalItems - 3)
        {
            await viewModel.LoadMoreNewsAsync();
        }
    }

    private async void OnNewsItemTapped(object sender, TappedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[News] Item tapped");
        
        if (sender is Border border && border.BindingContext is NewsItem newsItem)
        {
            System.Diagnostics.Debug.WriteLine($"[News] Selected: {newsItem.Title}");
            try
            {
                var detailPage = MauiProgram.Services.GetService<NewsDetailView>();
                if (detailPage != null)
                {
                    detailPage.LoadNewsItem(newsItem);
                    await Shell.Current.Navigation.PushAsync(detailPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[News] Navigation error: {ex.Message}");
            }
        }
    }

    private async void OnNewsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[News] SelectionChanged triggered");
        
        if (e.CurrentSelection.FirstOrDefault() is NewsItem newsItem)
        {
            System.Diagnostics.Debug.WriteLine($"[News] Selected: {newsItem.Title}");
            try
            {
                var detailPage = MauiProgram.Services.GetService<NewsDetailView>();
                if (detailPage != null)
                {
                    detailPage.LoadNewsItem(newsItem);
                    await Shell.Current.Navigation.PushAsync(detailPage);
                    System.Diagnostics.Debug.WriteLine("[News] Navigation success");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[News] detailPage is null");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[News] Navigation error: {ex.Message}");
            }
        }
        ((CollectionView)sender).SelectedItem = null;
    }
}