using EOM.TSHotelManagementSystem.Mobile.Service;
using Microsoft.Extensions.Logging;
using UraniumUI;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public static class MauiProgram
    {
        private static IServiceProvider _serviceProvider;
        public static IServiceProvider Services => _serviceProvider;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddMaterialSymbolsFonts();
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            RegisterServices(builder.Services);
            RegisterRoutes();

            var app = builder.Build();
            _serviceProvider = app.Services;

            return app;
        }

        private static void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(NewsView), typeof(NewsView));
            Routing.RegisterRoute(nameof(ProfileView), typeof(ProfileView));
            Routing.RegisterRoute(nameof(CheckInView), typeof(CheckInView));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IHttpService, HttpService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddTransient<MainPageViewModel>();
            services.AddTransient<NewsViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();

            services.AddTransient<CheckInView>();
            services.AddTransient<NewsView>();
            services.AddTransient<ProfileView>();
            services.AddTransient<LoginPage>();
            services.AddTransient<RegisterPage>();
            services.AddTransient<BottomNavigationBar>();

            services.AddSingleton<AppShell>(sp => new AppShell(
                sp.GetRequiredService<IAuthService>(),
                sp.GetRequiredService<INavigationService>()
            ));
            services.AddSingleton<MainPage>();
        }
    }
}
