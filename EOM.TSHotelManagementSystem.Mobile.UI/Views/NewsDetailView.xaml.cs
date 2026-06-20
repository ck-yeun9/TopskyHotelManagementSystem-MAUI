namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class NewsDetailView : ContentPage
{
    private readonly NewsDetailViewModel _viewModel;

    public NewsDetailView(NewsDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void LoadNewsItem(NewsItem newsItem)
    {
        _viewModel.LoadNewsItem(newsItem);
    }
}
