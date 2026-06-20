namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class NewsDetailView : ContentPage
{
    public NewsDetailView()
    {
        InitializeComponent();
    }

    public void SetNewsItem(NewsItem newsItem)
    {
        BindingContext = newsItem;
    }
}
