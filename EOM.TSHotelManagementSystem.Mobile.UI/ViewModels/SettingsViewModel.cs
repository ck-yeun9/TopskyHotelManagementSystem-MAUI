using EOM.TSHotelManagementSystem.Mobile.Service;
using Plugin.Fingerprint;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IThemeService _themeService;
        private readonly IAuthService _authService;
        private bool _isDarkMode;
        private bool _isBiometricEnabled;
        private bool _isBiometricAvailable;
        private bool _isLoading;
        private string _appVersion;

        public SettingsViewModel(IThemeService themeService, IAuthService authService)
        {
            _themeService = themeService;
            _authService = authService;
            _isDarkMode = _themeService.IsDarkMode;
            _appVersion = AppInfo.Current.VersionString;
            ToggleDarkModeCommand = new Command(OnToggleDarkMode);
            ClearCacheCommand = new Command(async () => await ClearCacheAsync());
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (SetField(ref _isDarkMode, value))
                {
                    _themeService.SetDarkMode(value);
                }
            }
        }

        public bool IsBiometricEnabled
        {
            get => _isBiometricEnabled;
            set
            {
                if (SetField(ref _isBiometricEnabled, value) && !_isLoading)
                {
                    _ = ToggleBiometricAsync(value);
                }
            }
        }

        public bool IsBiometricAvailable
        {
            get => _isBiometricAvailable;
            set => SetField(ref _isBiometricAvailable, value);
        }

        public string AppVersion { get => _appVersion; set => SetField(ref _appVersion, value); }

        public ICommand ToggleDarkModeCommand { get; }
        public ICommand ClearCacheCommand { get; }

        public async Task LoadSettingsAsync()
        {
            try
            {
                _isLoading = true;

                var isSupported = await CrossFingerprint.Current.IsAvailableAsync(false);
                var isLoggedIn = await _authService.HasValidTokenAsync();
                IsBiometricAvailable = isSupported && isLoggedIn;

                if (IsBiometricAvailable)
                {
                    IsBiometricEnabled = await _authService.IsBiometricEnabledAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadSettingsAsync Exception: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task ToggleBiometricAsync(bool isEnabled)
        {
            try
            {
                if (isEnabled)
                {
                    var isSupported = await CrossFingerprint.Current.IsAvailableAsync(false);
                    if (!isSupported)
                    {
                        IsBiometricEnabled = false;
                        await Shell.Current.DisplayAlert("提示", "您的设备不支持生物识别", "确定");
                        return;
                    }

                    var isLoggedIn = await _authService.HasValidTokenAsync();
                    if (!isLoggedIn)
                    {
                        IsBiometricEnabled = false;
                        await Shell.Current.DisplayAlert("提示", "请先登录后再开启生物识别", "确定");
                        return;
                    }

                    await Microsoft.Maui.Storage.SecureStorage.SetAsync("BiometricEnabled", "true");
                    await Shell.Current.DisplayAlert("提示", "生物识别已开启，下次登录可使用指纹/面容", "确定");
                }
                else
                {
                    Microsoft.Maui.Storage.SecureStorage.Remove("BiometricEnabled");
                    await _authService.ClearBiometricCredentialsAsync();
                    await Shell.Current.DisplayAlert("提示", "生物识别已关闭", "确定");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ToggleBiometricAsync Exception: {ex.Message}");
            }
        }

        private void OnToggleDarkMode()
        {
            IsDarkMode = !IsDarkMode;
        }

        private async Task ClearCacheAsync()
        {
            try
            {
                var cacheDir = FileSystem.CacheDirectory;
                if (Directory.Exists(cacheDir))
                {
                    foreach (var file in Directory.GetFiles(cacheDir))
                    {
                        File.Delete(file);
                    }
                }

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.DisplayAlert("提示", "缓存已清除", "确定");
                    }
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.DisplayAlert("错误", $"清除缓存失败: {ex.Message}", "确定");
                    }
                });
            }
        }
    }
}
