namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class RegisterPage : ContentPage
{
	public RegisterPage(RegisterViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is RegisterViewModel vm)
        {
            vm.ResetState();
        }
    }
}