namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class OrderCheckoutView : ContentPage
    {
        public OrderCheckoutView(OrderCheckoutViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
