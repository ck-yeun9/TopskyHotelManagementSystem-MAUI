namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class AppShell : Shell
    {
        public static readonly BindableProperty CurrentPageTitleProperty =
        BindableProperty.Create(nameof(CurrentPageTitle), typeof(string), typeof(AppShell), "酒店管理系统");

        public string CurrentPageTitle
        {
            get => (string)GetValue(CurrentPageTitleProperty);
            set => SetValue(CurrentPageTitleProperty, value);
        }

        public AppShell()
        {
            InitializeComponent();
            BindingContext = this;
            RegisterRoutes();

            this.Navigated += OnNavigated;
        }

        private void OnNavigated(object sender, ShellNavigatedEventArgs e)
        {
            if (CurrentPage != null)
            {
                CurrentPageTitle = CurrentPage.Title;
            }
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(DetailsPage), typeof(DetailsPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        }
    }
}
