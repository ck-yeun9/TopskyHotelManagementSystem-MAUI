using System.Reflection;
using System.Diagnostics;
using EOM.TSHotelManagementSystem.Mobile.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using UraniumUI;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public static class MauiProgram
    {
        private static IServiceProvider _serviceProvider;
        public static IServiceProvider Services => _serviceProvider;

        /// <summary>
        /// 临时调试用：将异常写入手机 Download 目录，闪退后用文件管理器找到 crash.log 发给我。
        /// 调试完毕后删除此方法和相关调用。
        /// </summary>
        private static void SetupCrashLogger()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                WriteCrashLog($"[AppDomain.UnhandledException] {ex}");
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                WriteCrashLog($"[UnobservedTaskException] {e.Exception}");
            };
        }

        private static void WriteCrashLog(string message)
        {
            try
            {
#if ANDROID
                var dir = Android.App.Application.Context.GetExternalFilesDir(null)!.AbsolutePath;
#else
                var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
#endif
                var logPath = Path.Combine(dir, "crash.log");
                var text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n\n";
                File.AppendAllText(logPath, text);
                Debug.WriteLine($"[CRASH LOG] {logPath}");
            }
            catch { }
        }

        public static MauiApp CreateMauiApp()
        {
            SetupCrashLogger();
            WriteCrashLog("=== CreateMauiApp started ===");
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

            // 加载 UI 层 appsettings.json（内嵌资源）作为统一配置源
            var configuration = BuildConfiguration();
            builder.Services.AddSingleton<IConfiguration>(configuration);

            // 将 Http 相关配置绑定为强类型选项并注册，Service 层不再写死 BaseUrl 等
            var baseUrl = configuration["Http:BaseUrl"]?.Trim();
            var timeoutRaw = configuration["Http:TimeoutSeconds"];
            var userAgent = configuration["Http:UserAgent"];

            var httpOptions = new HttpOptions
            {
                BaseUrl = string.IsNullOrEmpty(baseUrl)
                          ? "https://tshotel-debug.oscode.top/api/"
                          : baseUrl,
                TimeoutSeconds = int.TryParse(timeoutRaw, out var t) && t > 0 ? t : 30,
                UserAgent = string.IsNullOrEmpty(userAgent)
                          ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36"
                          : userAgent
            };
            builder.Services.AddSingleton(httpOptions);

            RegisterServices(builder.Services);
            RegisterRoutes();

            var app = builder.Build();
            _serviceProvider = app.Services;
            WriteCrashLog("=== CreateMauiApp completed ===");

            return app;
        }

        /// <summary>
        /// 从内嵌的配置资源构建 IConfiguration：
        /// 先加载基础 appsettings.json，再按当前构建环境叠加 appsettings.{Environment}.json
        /// （Debug -> tshotel-debug，Release -> tshotel），环境文件中的配置优先级更高。
        /// 若资源缺失则回退到空配置（此时由强类型默认值兜底）。
        /// </summary>
        private static IConfiguration BuildConfiguration()
        {
            var assembly = typeof(MauiProgram).Assembly;
            var configBuilder = new ConfigurationBuilder();

            using (var baseStream = assembly.GetManifestResourceStream("appsettings.json"))
            {
                if (baseStream is not null)
                {
                    var ms = new MemoryStream();
                    baseStream.CopyTo(ms);
                    ms.Position = 0;
                    configBuilder.AddJsonStream(ms);
                }
            }

            // 按构建环境选择对应的环境配置文件
            var env = IsDebug ? "Debug" : "Release";
            using (var envStream = assembly.GetManifestResourceStream($"appsettings.{env}.json"))
            {
                if (envStream is not null)
                {
                    var ms = new MemoryStream();
                    envStream.CopyTo(ms);
                    ms.Position = 0;
                    configBuilder.AddJsonStream(ms);
                }
            }

            return configBuilder.Build();
        }

#if DEBUG
        private const bool IsDebug = true;
#else
        private const bool IsDebug = false;
#endif

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
