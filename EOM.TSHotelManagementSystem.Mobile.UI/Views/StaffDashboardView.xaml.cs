using EOM.TSHotelManagementSystem.Mobile.Service;
using Microsoft.Extensions.DependencyInjection;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class StaffDashboardView : ContentView
    {
        public StaffDashboardView()
        {
            InitializeComponent();
        }

        protected override async void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            if (Handler != null)
            {
                await CheckStaffAuthAsync();
            }
        }

        private async Task CheckStaffAuthAsync()
        {
            try
            {
                var staffService = MauiProgram.Services.GetRequiredService<IStaffService>();
                var hasToken = await staffService.HasValidStaffTokenAsync();
                LoginButton.IsVisible = !hasToken;
            }
            catch
            {
                LoginButton.IsVisible = true;
            }
        }

        private async void OnStaffLoginClicked(object sender, EventArgs e)
        {
            var navService = MauiProgram.Services.GetRequiredService<INavigationService>();
            await navService.NavigateToAsync(nameof(StaffLoginPage));
        }

        private async void OnRoomStatusTapped(object sender, EventArgs e)
        {
            if (!await CheckAuthAndAlertAsync()) return;
            var navService = MauiProgram.Services.GetRequiredService<INavigationService>();
            await navService.NavigateToAsync(nameof(RoomStatusView));
        }

        private async void OnMeterReadingTapped(object sender, EventArgs e)
        {
            if (!await CheckAuthAndAlertAsync()) return;
            var navService = MauiProgram.Services.GetRequiredService<INavigationService>();
            await navService.NavigateToAsync(nameof(MeterReadingView));
        }

        private async void OnMaterialTapped(object sender, EventArgs e)
        {
            if (!await CheckAuthAndAlertAsync()) return;
            var navService = MauiProgram.Services.GetRequiredService<INavigationService>();
            await navService.NavigateToAsync(nameof(MaterialManagementView));
        }

        private async Task<bool> CheckAuthAndAlertAsync()
        {
            var staffService = MauiProgram.Services.GetRequiredService<IStaffService>();
            if (!await staffService.HasValidStaffTokenAsync())
            {
                await Application.Current.MainPage.DisplayAlert("提示", "请先进行员工登录", "确定");
                return false;
            }
            return true;
        }
    }
}
