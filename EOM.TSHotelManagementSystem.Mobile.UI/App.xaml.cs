using EOM.TSHotelManagementSystem.Mobile.Service;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            var serviceProvider = MauiProgram.Services;

            MainPage = serviceProvider.GetRequiredService<AppShell>();
        }
    }
}
