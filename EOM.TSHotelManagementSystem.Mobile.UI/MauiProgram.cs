using Microsoft.Extensions.Logging;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<INavigationService, NavigationService>();

            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddTransient<DetailsPageViewModel>();

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<DetailsPage>();
            builder.Services.AddTransient<CheckInView>();
            builder.Services.AddTransient<NewsView>();
            builder.Services.AddTransient<ProfileView>();

            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}
