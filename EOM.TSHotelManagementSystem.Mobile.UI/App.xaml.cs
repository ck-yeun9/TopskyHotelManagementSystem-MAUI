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

            MainPage = serviceProvider.GetRequiredService<AppShell>();

            _ = ValidateTokenOnStartupAsync(serviceProvider);
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
