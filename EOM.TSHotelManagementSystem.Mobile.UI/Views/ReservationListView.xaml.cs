namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class ReservationListView : ContentPage
{
    private readonly ReservationListViewModel _viewModel;

    public ReservationListView(ReservationListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.Current.Title = "我的预约";
        _viewModel.OnViewAppearing();
    }
}
