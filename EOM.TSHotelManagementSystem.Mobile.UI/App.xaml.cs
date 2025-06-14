namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            MainPage = serviceProvider.GetService<AppShell>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<MainPageViewModel>();
            services.AddTransient<DetailsPageViewModel>();
            services.AddSingleton<AppShell>();
        }
    }
}
