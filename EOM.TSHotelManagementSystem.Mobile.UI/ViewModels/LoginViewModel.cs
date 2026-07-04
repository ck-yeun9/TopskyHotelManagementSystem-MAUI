using EOM.TSHotelManagementSystem.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;

            LoginCommand = new Command(async () => await LoginAsync());
            NavigateToRegisterCommand = new Command(NavigateToRegister);
            SkipLoginCommand = new Command(async () => await SkipLoginAsync());
            BiometricLoginCommand = new Command(async () => await BiometricLoginAsync());
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetField(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetField(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }

        private bool _isbusy;
        public bool IsBusy
        {
            get => _isbusy;
            set => SetField(ref _isbusy, value);
        }

        private bool _isBiometricAvailable;
        public bool IsBiometricAvailable
        {
            get => _isBiometricAvailable;
            set => SetField(ref _isBiometricAvailable, value);
        }

        private string _biometricButtonText = "指纹登录";
        public string BiometricButtonText
        {
            get => _biometricButtonText;
            set => SetField(ref _biometricButtonText, value);
        }

        public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand SkipLoginCommand { get; }
        public ICommand BiometricLoginCommand { get; }

        public async Task CheckBiometricAvailabilityAsync()
        {
            try
            {
                var isEnabled = await _authService.IsBiometricEnabledAsync();
                var isSupported = await Plugin.Fingerprint.CrossFingerprint.Current.IsAvailableAsync(false);
                IsBiometricAvailable = isEnabled && isSupported;

                if (IsBiometricAvailable)
                {
                    BiometricButtonText = "指纹/面容登录";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查生物识别失败: {ex.Message}");
                IsBiometricAvailable = false;
            }
        }

        public void ResetState()
        {
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
            IsBusy = false;
        }

        private async Task LoginAsync()
        {
            IsBusy = true;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "用户名和密码不能为空";
                IsBusy = false;
                return;
            }

            var success = await _authService.LoginAsync(Username, Password);

            if (success)
            {
                var savedUsername = Username;
                var savedPassword = Password;

                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");

                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await CheckAndEnableBiometricAsync(savedUsername, savedPassword);
                    });
                });
            }
            else
            {
                ErrorMessage = "登录失败，请检查用户名和密码";
            }
            IsBusy = false;
        }

        private async Task CheckAndEnableBiometricAsync(string username, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Biometric] 开始检查生物识别...");
                
                var isBiometricSupported = await Plugin.Fingerprint.CrossFingerprint.Current.IsAvailableAsync(false);
                System.Diagnostics.Debug.WriteLine($"[Biometric] 设备支持: {isBiometricSupported}");
                
                var isBiometricEnabled = await _authService.IsBiometricEnabledAsync();
                System.Diagnostics.Debug.WriteLine($"[Biometric] 已启用: {isBiometricEnabled}");

                if (!isBiometricSupported)
                {
                    System.Diagnostics.Debug.WriteLine("[Biometric] 设备不支持，跳过");
                    return;
                }

                if (isBiometricEnabled)
                {
                    System.Diagnostics.Debug.WriteLine("[Biometric] 已启用，更新凭据");
                    await _authService.SaveBiometricCredentialsAsync(username, password);
                    return;
                }

                System.Diagnostics.Debug.WriteLine("[Biometric] 弹出询问对话框...");
                var enable = await Shell.Current.DisplayAlertAsync("登录成功", "是否开启指纹/面容登录？下次可快速登录", "开启", "暂不");
                System.Diagnostics.Debug.WriteLine($"[Biometric] 用户选择: {enable}");

                if (!enable) return;

                System.Diagnostics.Debug.WriteLine("[Biometric] 调用生物识别验证...");
                var authResult = await Plugin.Fingerprint.CrossFingerprint.Current.AuthenticateAsync(
                    new Plugin.Fingerprint.Abstractions.AuthenticationRequestConfiguration("验证身份", "请验证指纹或面容以开启生物识别登录"));
                
                System.Diagnostics.Debug.WriteLine($"[Biometric] 验证结果: {authResult.Authenticated}, {authResult.Status}");

                if (authResult.Authenticated)
                {
                    await _authService.SaveBiometricCredentialsAsync(username, password);
                    await Microsoft.Maui.Storage.SecureStorage.SetAsync("BiometricEnabled", "true");
                    await Shell.Current.DisplayAlertAsync("开启成功", "已开启生物识别登录，下次可使用指纹/面容快速登录", "确定");
                    System.Diagnostics.Debug.WriteLine("[Biometric] 开启成功");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Biometric] 验证失败: {authResult.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Biometric] 异常: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Biometric] 堆栈: {ex.StackTrace}");
            }
        }

        private async Task BiometricLoginAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var success = await _authService.LoginWithBiometricAsync();

            if (success)
            {
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                ErrorMessage = "生物识别登录失败，请使用密码登录";
            }
            IsBusy = false;
        }

        private async Task SkipLoginAsync()
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }

        private async void NavigateToRegister()
        {
            try
            {
                await _navigationService.NavigateToAsync($"//{nameof(RegisterPage)}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"无法打开注册页面: {ex.Message}";
            }
        }
    }
}
