using Microsoft.Maui.Controls;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class StaffLoginPage : ContentPage
    {
        public StaffLoginPage(StaffLoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is StaffLoginViewModel vm)
            {
                vm.ResetState();
            }
        }
    }
}
