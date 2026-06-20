using EOM.TSHotelManagementSystem.Mobile.Service;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        private string _activeTab = "checkin";
        private string _appName = "TopSky酒店";

        public MainPageViewModel(
            INavigationService navigationService,
            IAuthService authService)
        {
            _navigationService = navigationService;
            _authService = authService;
        }

        public string AppName
        {
            get => _appName;
            set => SetField(ref _appName, value);
        }

        public string ActiveTab
        {
            get => _activeTab;
            set
            {
                if (SetField(ref _activeTab, value))
                {
                    UpdateTitle();
                }
            }
        }

        private string _currentTitle = "TopSky酒店";
        public string CurrentTitle
        {
            get => _currentTitle;
            set => SetField(ref _currentTitle, value);
        }

        private void UpdateTitle()
        {
            CurrentTitle = ActiveTab switch
            {
                "news" => "最新资讯",
                "checkin" => "入住管理",
                "profile" => "个人中心",
                _ => AppName
            };

            var appShell = Application.Current?.MainPage as AppShell;
            appShell?.Title = CurrentTitle;
        }
    }
}