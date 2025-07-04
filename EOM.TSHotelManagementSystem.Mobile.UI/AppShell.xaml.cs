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
                _authService?.ClearToken();
                await Current.GoToAsync($"//{nameof(LoginPage)}");
            });
        }

        private async void OnNavigated(object sender, ShellNavigatedEventArgs e)
        {
            if (e.Current.Location.OriginalString.Contains(nameof(LoginPage)))
                return;

            await Task.Delay(300);

            if (!_authService.HasValidToken())
            {
                await Current.GoToAsync($"//{nameof(LoginPage)}");
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