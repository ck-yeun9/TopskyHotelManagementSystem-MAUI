
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