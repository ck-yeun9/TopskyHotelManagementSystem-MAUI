namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class DetailsPage : ContentPage
{
	public DetailsPage(DetailsPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        Title = "ฯ๊ว้าณ";
    }
}