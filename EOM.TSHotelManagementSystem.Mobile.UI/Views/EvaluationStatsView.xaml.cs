namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class EvaluationStatsView : ContentPage
{
    private readonly EvaluationStatsViewModel _viewModel;

    public EvaluationStatsView(EvaluationStatsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void Initialize(string roomNumber, string roomName, string stayId)
    {
        _viewModel.Init(roomNumber, roomName, stayId);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnViewAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnViewDisappearing();
    }
}
