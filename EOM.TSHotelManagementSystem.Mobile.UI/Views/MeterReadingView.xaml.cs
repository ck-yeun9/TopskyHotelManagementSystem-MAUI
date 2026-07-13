using Microsoft.Maui.Controls;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class MeterReadingView : ContentPage
    {
        public MeterReadingView(MeterReadingViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ILoadableViewModel loadable)
            {
                loadable.OnViewAppearing();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (BindingContext is ILoadableViewModel loadable)
            {
                loadable.OnViewDisappearing();
            }
        }
    }
}
