using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IThemeService _themeService;
        private bool _isDarkMode;
        private string _appVersion;

        public SettingsViewModel(IThemeService themeService)
        {
            _themeService = themeService;
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

        public string AppVersion { get => _appVersion; set => SetField(ref _appVersion, value); }

        public ICommand ToggleDarkModeCommand { get; }
        public ICommand ClearCacheCommand { get; }

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
