using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class PersonalInfoViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IProfileService _profileService;
        private readonly IAuthService _authService;

        private string _displayName;
        private string _account;
        private string _photoUrl;
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string _statusMessage;
        private string _statusMessageColor = "Green";
        private bool _isBusy;
        private bool _isChangingPassword;

        public PersonalInfoViewModel(IProfileService profileService, IAuthService authService)
        {
            _profileService = profileService;
            _authService = authService;
            LoadProfileCommand = new Command(async () => await LoadProfileAsync());
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            TogglePasswordSectionCommand = new Command(() => IsChangingPassword = !IsChangingPassword);
        }

        public string DisplayName { get => _displayName; set => SetField(ref _displayName, value); }
        public string Account { get => _account; set => SetField(ref _account, value); }
        public string PhotoUrl { get => _photoUrl; set => SetField(ref _photoUrl, value); }
        public string CurrentPassword { get => _currentPassword; set => SetField(ref _currentPassword, value); }
        public string NewPassword { get => _newPassword; set => SetField(ref _newPassword, value); }
        public string ConfirmPassword { get => _confirmPassword; set => SetField(ref _confirmPassword, value); }
        public string StatusMessage { get => _statusMessage; set => SetField(ref _statusMessage, value); }
        public string StatusMessageColor { get => _statusMessageColor; set => SetField(ref _statusMessageColor, value); }
        public bool IsBusy { get => _isBusy; set => SetField(ref _isBusy, value); }
        public bool IsChangingPassword { get => _isChangingPassword; set => SetField(ref _isChangingPassword, value); }

        public ICommand LoadProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand TogglePasswordSectionCommand { get; }

        public async void OnViewAppearing()
        {
            System.Diagnostics.Debug.WriteLine("PersonalInfoViewModel OnViewAppearing");
            await LoadProfileAsync();
        }

        public void OnViewDisappearing()
        {
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PersonalInfoViewModel LoadProfileAsync");
                var profile = await _profileService.GetUserProfileAsync();
                System.Diagnostics.Debug.WriteLine($"PersonalInfoViewModel Profile={profile != null}");
                
                if (profile != null)
                {
                    DisplayName = profile.DisplayName ?? profile.Account;
                    Account = profile.Account;
                    PhotoUrl = profile.PhotoUrl;
                    System.Diagnostics.Debug.WriteLine($"PersonalInfoViewModel: DisplayName={DisplayName}, Account={Account}");
                }
                else
                {
                    StatusMessage = "无法加载个人信息";
                    StatusMessageColor = "Red";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PersonalInfoViewModel Exception: {ex.Message}");
                StatusMessage = $"加载资料失败: {ex.Message}";
                StatusMessageColor = "Red";
            }
        }

        private async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                StatusMessage = "请填写所有密码字段";
                StatusMessageColor = "Red";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                StatusMessage = "两次输入的新密码不一致";
                StatusMessageColor = "Red";
                return;
            }

            if (NewPassword.Length < 6)
            {
                StatusMessage = "新密码长度至少为6位";
                StatusMessageColor = "Red";
                return;
            }

            try
            {
                IsBusy = true;
                var httpService = MauiProgram.Services.GetService<IHttpService>();
                var json = HttpHelper.ModelToJson(new { OldPassword = CurrentPassword, NewPassword = NewPassword, ConfirmPassword = ConfirmPassword });
                var response = await httpService.RequestAsync("Profile/ChangePassword", json: json);

                if (string.IsNullOrWhiteSpace(response?.Message))
                {
                    StatusMessage = "服务器无响应";
                    StatusMessageColor = "Red";
                    return;
                }

                var result = HttpHelper.JsonToModel<ApiResponse>(response.Message);

                if (result?.Code == 0)
                {
                    StatusMessage = "密码修改成功";
                    StatusMessageColor = "Green";
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;
                    IsChangingPassword = false;
                }
                else
                {
                    StatusMessage = result?.Message ?? "密码修改失败";
                    StatusMessageColor = "Red";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"密码修改失败: {ex.Message}";
                StatusMessageColor = "Red";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
