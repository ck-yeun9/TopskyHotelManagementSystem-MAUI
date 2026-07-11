namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class EvaluationView : ContentPage
{
    private readonly EvaluationViewModel _viewModel;

    public EvaluationView(EvaluationViewModel viewModel)
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
