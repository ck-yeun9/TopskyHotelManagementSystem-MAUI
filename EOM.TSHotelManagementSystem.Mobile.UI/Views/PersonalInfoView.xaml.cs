namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class PersonalInfoView : ContentPage
{
    private readonly PersonalInfoViewModel _viewModel;

    public PersonalInfoView(PersonalInfoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ILoadableViewModel loadable)
        {
            loadable.OnViewAppearing();
        }
    }
}
