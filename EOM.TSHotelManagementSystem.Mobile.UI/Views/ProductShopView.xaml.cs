namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class ProductShopView : ContentPage
    {
        private readonly ProductShopViewModel _viewModel;

        public ProductShopView(ProductShopViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Shell.Current.Title = "商品消费";
            try
            {
                await _viewModel.LoadDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnAppearing Exception: {ex}");
                await DisplayAlertAsync("页面加载错误", ex.ToString(), "确定");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Shell.Current.Title = "商品消费";
        }
    }
}
