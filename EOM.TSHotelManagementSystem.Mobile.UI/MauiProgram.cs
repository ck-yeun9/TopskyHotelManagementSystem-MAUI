using EOM.TSHotelManagementSystem.Mobile.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
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

            // 去掉 Android Entry 的默认下划线，避免与自定义 Border 外框重叠
            EntryHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.Background = null;
#endif
            });

            // 原生 DatePicker 替换为自带橙色主题的日历弹窗
            DatePickerHandler.Mapper.AppendToMapping("OrangeDatePicker", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.SetOnClickListener(null);
                handler.PlatformView.Click += (s, e) =>
                {
                    var dlg = new Android.App.DatePickerDialog(handler.PlatformView.Context!);
                    dlg.DatePicker!.DateTime = (DateTime)view.Date;
                    dlg.SetButton(-1, "确定", (_, _) =>
                    {
                        var dp = dlg.DatePicker!;
                        view.Date = new DateTime(dp.DateTime.Year, dp.DateTime.Month, dp.DateTime.Day);
                    });
                    dlg.SetButton(-2, "取消", (_, _) => { });
                    dlg.Show();
                    dlg.GetButton(-1)?.SetTextColor(Android.Graphics.Color.ParseColor("#FF5722"));
                    dlg.GetButton(-2)?.SetTextColor(Android.Graphics.Color.ParseColor("#FF5722"));
                };
#endif
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
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(ReservationListView), typeof(ReservationListView));
            Routing.RegisterRoute(nameof(NewsDetailView), typeof(NewsDetailView));
            Routing.RegisterRoute(nameof(PersonalInfoView), typeof(PersonalInfoView));
            Routing.RegisterRoute(nameof(SettingsView), typeof(SettingsView));
            Routing.RegisterRoute(nameof(EvaluationStatsView), typeof(EvaluationStatsView));
            Routing.RegisterRoute(nameof(EvaluationView), typeof(EvaluationView));
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IBookingService, RealBookingService>();
            services.AddSingleton<IHttpService, HttpService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INewsService, NewsService>();
            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<IReservationService, ReservationService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IShopService, ShopService>();
            services.AddSingleton<IEvaluationService, EvaluationService>();

            services.AddTransient<MainPageViewModel>();
            services.AddTransient<CheckInViewModel>();
            services.AddTransient<NewsViewModel>();
            services.AddTransient<NewsDetailViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<ReservationListViewModel>();
            services.AddTransient<PersonalInfoViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<ProductShopViewModel>();
            services.AddTransient<OrderCheckoutViewModel>();
            services.AddSingleton<EvaluationStatsViewModel>();
            services.AddTransient<EvaluationViewModel>();

            services.AddTransient<CheckInView>();
            services.AddTransient<NewsView>();
            services.AddTransient<NewsDetailView>();
            services.AddTransient<ProfileView>();
            services.AddTransient<LoginPage>();
            services.AddTransient<RegisterPage>();
            services.AddTransient<BottomNavigationBar>();
            services.AddTransient<ReservationListView>();
            services.AddTransient<NewsDetailView>();
            services.AddTransient<PersonalInfoView>();
            services.AddTransient<SettingsView>();
            services.AddTransient<ProductShopView>();
            services.AddTransient<OrderCheckoutView>();
            services.AddTransient<EvaluationStatsView>();
            services.AddTransient<EvaluationView>();

            services.AddSingleton<AppShell>(sp => new AppShell(
                sp.GetRequiredService<IAuthService>(),
                sp.GetRequiredService<INavigationService>()
            ));
            services.AddSingleton<MainPage>();
        }
    }
}
