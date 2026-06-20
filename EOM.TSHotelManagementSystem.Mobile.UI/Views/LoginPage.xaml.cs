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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (Shell.Current != null)
            {
                Shell.Current.Title = "欢迎使用，请先登录";
            }

            if (BindingContext is LoginViewModel vm)
            {
                vm.ResetState();
                await vm.CheckBiometricAvailabilityAsync();
            }
        }
    }
}