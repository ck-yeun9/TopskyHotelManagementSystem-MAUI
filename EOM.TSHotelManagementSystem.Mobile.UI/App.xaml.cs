namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            InitializeComponent();

            var mauiApp = MauiProgram.CreateMauiApp();
            Services = mauiApp.Services;
            MainPage = Services.GetService<AppShell>();
        }
    }
}
