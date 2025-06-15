using static System.Collections.Specialized.NameObjectCollectionBase;

namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class NewsView : ContentView
{
    public NewsView()
    {
        InitializeComponent(); 
        
        var viewModel = App.Services.GetRequiredService<NewsViewModel>();

        BindingContext = viewModel;

        if (viewModel.NewsItems.Count == 0 && !viewModel.IsRefreshing && !viewModel.IsInitializing)
        {
            _ = viewModel.LoadNewsAsync();
            viewModel.IsInitializing = true;
        }

        if (NewsCollection != null)
        {
            NewsCollection.Scrolled += OnNewsCollectionScrolled;
        }
    }

    /// <summary>
    /// 滚动到底部加载更多
    /// </summary>
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
}