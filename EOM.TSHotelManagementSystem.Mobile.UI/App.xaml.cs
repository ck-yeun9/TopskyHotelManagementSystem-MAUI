using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class App : Application
    {

        private static void Log(string msg) =>
            System.Diagnostics.Debug.WriteLine($"[TOPSY] {msg}");

        public App()
        {
            Log("App() constructor START");
            InitializeComponent();
            Log("App() InitializeComponent done");

            var serviceProvider = MauiProgram.Services;
            serviceProvider.GetRequiredService<IThemeService>().ApplyTheme();
            Log("App() constructor END");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Log("CreateWindow START");
            var window = new Window(BuildStartupSplashPage());
            Log("CreateWindow splash page created");
            _ = RunStartupAsync(MauiProgram.Services);
            Log("CreateWindow END");
            return window;
        }

        private async Task RunStartupAsync(IServiceProvider serviceProvider)
        {
            Log("RunStartupAsync START");
            try
            {
                var httpService = serviceProvider.GetRequiredService<IHttpService>();
                Log("RunStartupAsync calling /version");
                var response = await httpService.RequestAsync("version");
                Log($"RunStartupAsync /version response: {response?.StatusCode}");

                if (response?.StatusCode == 200)
                {
                    Log("RunStartupAsync 200 OK, setting AppShell");
                    SetRootPage(serviceProvider.GetRequiredService<AppShell>());
                    _ = ValidateTokenOnStartupAsync(serviceProvider);
                    return;
                }
                Log($"RunStartupAsync non-200: {response?.StatusCode}");
            }
            catch (Exception ex)
            {
                Log($"RunStartupAsync EXCEPTION: {ex}");
            }

            Log("RunStartupAsync showing StartupUnavailablePage");
            SetRootPage(new StartupUnavailablePage(async () =>
                await RunStartupAsync(serviceProvider)));
        }

        private void SetRootPage(Page page)
        {
            Log($"SetRootPage: Windows.Count={Windows.Count}, page={page.GetType().Name}");
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
