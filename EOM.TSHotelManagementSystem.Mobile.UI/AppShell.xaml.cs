using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class AppShell : Shell
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navService;

        public AppShell(IAuthService authService, INavigationService navService)
        {
            InitializeComponent();

            _authService = authService;
            _navService = navService;

            InitLogoutCommand();
        }

        private void InitLogoutCommand()
        {
            LogoutCommand = new Command(async () => {
                await _authService.ClearTokenAsync();
                await Current.GoToAsync($"//{nameof(MainPage)}");
            });
        }

        public ICommand LogoutCommand { get; private set; }
    }
}