using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class ProfileViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IProfileService _profileService;
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetField(ref _userName, value);
        }

        private string _userAccount;
        public string UserAccount
        {
            get => _userAccount;
            set => SetField(ref _userAccount, value);
        }

        private string _userLevel;
        public string UserLevel
        {
            get => _userLevel;
            set => SetField(ref _userLevel, value);
        }

        private string _photoUrl;
        public string PhotoUrl
        {
            get => _photoUrl;
            set
            {
                if (SetField(ref _photoUrl, value))
                {
                    OnPropertyChanged(nameof(HasPhoto));
                }
            }
        }

        public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoUrl);

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                SetField(ref _isLoggedIn, value);
                OnPropertyChanged(nameof(IsNotLoggedIn));
            }
        }

        public bool IsNotLoggedIn => !IsLoggedIn;

        public ICommand NavigateToPersonalInfoCommand { get; private set; }
        public ICommand NavigateToSettingsCommand { get; private set; }
        public ICommand NavigateToReservationsCommand { get; private set; }
        public ICommand NavigateToLoginCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        public ProfileViewModel(IProfileService profileService, IAuthService authService, INavigationService navigationService)
        {
            _profileService = profileService;
            _authService = authService;
            _navigationService = navigationService;

            NavigateToPersonalInfoCommand = new Command(async () => await NavigateToPersonalInfo());
            NavigateToSettingsCommand = new Command(async () => await NavigateToSettings());
            NavigateToReservationsCommand = new Command(async () => await NavigateToReservations());
            NavigateToLoginCommand = new Command(async () => await NavigateToLogin());
            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        public async void OnViewAppearing()
        {
            await LoadUserDataAsync();
        }

        private async Task LoadUserDataAsync()
        {
            try
            {
                IsLoggedIn = await _authService.HasValidTokenAsync();
                System.Diagnostics.Debug.WriteLine($"LoadUserDataAsync: IsLoggedIn={IsLoggedIn}");
                
                if (IsLoggedIn)
                {
                    var profile = await _profileService.GetUserProfileAsync();
                    System.Diagnostics.Debug.WriteLine($"LoadUserDataAsync: Profile={profile != null}");
                    
                    if (profile != null)
                    {
                        UserName = profile.DisplayName ?? profile.Account;
                        UserAccount = profile.Account;
                        PhotoUrl = profile.PhotoUrl;
                        UserLevel = "会员";
                        System.Diagnostics.Debug.WriteLine($"LoadUserDataAsync: UserName={UserName}, UserAccount={UserAccount}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("LoadUserDataAsync: Profile is null");
                        UserName = "未知用户";
                        UserAccount = "加载失败";
                    }
                }
                else
                {
                    UserName = "游客";
                    UserAccount = "未登录";
                    PhotoUrl = null;
                    UserLevel = "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载用户资料失败: {ex.Message}");
                UserName = "加载错误";
                UserAccount = ex.Message;
            }
        }

        private async Task NavigateToPersonalInfo()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(PersonalInfoView));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("错误", $"导航失败: {ex.Message}", "确定");
            }
        }

        private async Task NavigateToSettings()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(SettingsView));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("错误", $"导航失败: {ex.Message}", "确定");
            }
        }

        private async Task NavigateToReservations()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(ReservationListView));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("错误", $"导航失败: {ex.Message}", "确定");
            }
        }

        private async Task NavigateToLogin()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(LoginPage));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("错误", $"导航失败: {ex.Message}", "确定");
            }
        }

        private async Task LogoutAsync()
        {
            await _authService.ClearTokenAsync();
            IsLoggedIn = false;
            UserName = "游客";
            UserAccount = "未登录";
            PhotoUrl = null;
            UserLevel = "";
        }

        public void OnViewDisappearing()
        {
        }
    }
}
