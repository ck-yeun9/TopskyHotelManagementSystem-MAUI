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

            this.Navigated += OnNavigated;
        }

        private void InitLogoutCommand()
        {
            LogoutCommand = new Command(async () => {
                await _authService.ClearTokenAsync();
                await Current.GoToAsync($"//{nameof(LoginPage)}");
            });
        }

        private async void OnNavigated(object sender, ShellNavigatedEventArgs e)
        {
            if (e.Current.Location.OriginalString.Contains(nameof(LoginPage)))
                return;

            if (!e.Previous?.Location.OriginalString.Contains(nameof(LoginPage)) ?? true)
            {
                if (!await _authService.ValidateAccessTokenAsync())
                {
                    await Current.GoToAsync($"//{nameof(LoginPage)}");
                    return;
                }
            }
        }

        public ICommand LogoutCommand { get; private set; }

        public void UpdateTitle(string title)
        {
            if (BindingContext is MainPageViewModel vm)
            {
                vm.CurrentTitle = title;
            }
        }

    }
}