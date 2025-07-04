using EOM.TSHotelManagementSystem.Mobile.Service;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            var serviceProvider = MauiProgram.Services;

            var authService = serviceProvider.GetRequiredService<IAuthService>();

            if (authService.HasValidToken())
            {
                MainPage = serviceProvider.GetRequiredService<AppShell>();
            }
            else
            {
                MainPage = new NavigationPage(serviceProvider.GetRequiredService<LoginPage>());
            }
        }
    }
}
