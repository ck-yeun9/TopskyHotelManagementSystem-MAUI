using Microsoft.Maui.Controls;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is LoginViewModel vm)
            {
                vm.ResetState();
            }
        }
    }
}