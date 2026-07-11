using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            var serviceProvider = MauiProgram.Services;
            serviceProvider.GetRequiredService<IThemeService>().ApplyTheme();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(BuildStartupSplashPage());
            _ = RunStartupAsync(MauiProgram.Services);
            return window;
        }

        private async Task RunStartupAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var httpService = serviceProvider.GetRequiredService<IHttpService>();
                var response = await httpService.RequestAsync("version");

                if (response?.StatusCode == 200)
                {
                    SetRootPage(serviceProvider.GetRequiredService<AppShell>());
                    _ = ValidateTokenOnStartupAsync(serviceProvider);
                    return;
                }
            }
            catch
            {
                // RequestAsync 内部 CheckNetworkStatus 也可能抛异常，一并走到这里。
            }

            SetRootPage(new StartupUnavailablePage(async () =>
                await RunStartupAsync(serviceProvider)));
        }

        private void SetRootPage(Page page)
        {
            if (Windows.Count > 0)
                Windows[0].Page = page;
        }

        private static ContentPage BuildStartupSplashPage()
        {
            return new ContentPage
            {
                Title = "正在连接服务…",
                BackgroundColor = Color.FromArgb("#FF5722"),
                Content = new VerticalStackLayout
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 16,
                    Children =
                    {
                        new ActivityIndicator
                        {
                            IsRunning = true,
                            Color = Colors.White,
                            WidthRequest = 56,
                            HeightRequest = 56,
                            HorizontalOptions = LayoutOptions.Center,
                        },
                        new Label
                        {
                            Text = "正在连接服务…",
                            FontSize = 18,
                            TextColor = Colors.White,
                            HorizontalOptions = LayoutOptions.Center,
                        }
                    }
                }
            };
        }

        private async Task ValidateTokenOnStartupAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var authService = serviceProvider.GetRequiredService<IAuthService>();
                var httpService = serviceProvider.GetRequiredService<IHttpService>();

                var hasToken = await authService.HasValidTokenAsync();
                if (!hasToken) return;

                var response = await httpService.RequestAsync("Profile/GetUserProfile");

                if (string.IsNullOrWhiteSpace(response?.Message))
                {
                    await authService.ClearTokenAsync();
                    return;
                }

                var result = HttpHelper.JsonToModel<SingleOutputDto<UserProfileOutputDto>>(response.Message);

                if (result?.Code != 0)
                {
                    await authService.ClearTokenAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ValidateTokenOnStartupAsync: {ex.Message}");
            }
        }
    }
}
