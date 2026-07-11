using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task NavigateToAsync(string route)
        {
            try
            {
                if (IsGlobalRoute(route))
                {
                    await HandleGlobalRouteNavigation(route);
                }
                else if (Application.Current?.MainPage is Shell shell)
                {
                    await shell.GoToAsync(route);
                }
                else
                {
                    await InitializeShellAndNavigateAsync(route);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"导航错误: {ex.Message}");
            }
        }

        public Task GoBackAsync()
        {
            try
            {
                if (Application.Current?.MainPage is Shell shell)
                {
                    return shell.GoToAsync("..");
                }
                else
                {
                    if (Application.Current?.MainPage is Page page)
                    {
                        return page.Navigation.PopAsync();
                    }
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"返回错误: {ex.Message}");
                return Task.CompletedTask;
            }
        }

        private bool IsGlobalRoute(string route)
        {
            var globalRoutes = new[]
            {
                nameof(LoginPage),
                nameof(RegisterPage)
            };

            return globalRoutes.Any(r =>
                route.Contains(r, StringComparison.OrdinalIgnoreCase) ||
                route.EndsWith($"/{r}", StringComparison.OrdinalIgnoreCase));
        }
        
        private async Task HandleGlobalRouteNavigation(string route)
        {
            try
            {
                // 全局路由（Login/Register）一律走 Shell 导航，绝不把 MainPage 整体换成
                // NavigationPage —— 否则 Shell.Current 会变 null，导致后续 Shell.Current.GoToAsync(...)
                // （登录成功、生物识别弹窗等）抛 NullReferenceException 并卡死主线程。
                var shell = Application.Current?.MainPage as Shell;
                if (shell == null)
                {
                    // 极少数情况下 Shell 已被替换（例如早期代码曾把 MainPage 换成 NavigationPage），
                    // 这里重建 Shell 作为导航根，恢复 Shell.Current。
                    shell = _serviceProvider.GetRequiredService<AppShell>();
                    Application.Current!.MainPage = shell;
                }

                // Login/Register 是用 Routing.RegisterRoute 注册的页面，必须用「相对路由」推入 Shell
                // （即 "LoginPage"/"RegisterPage"），不能用 "//LoginPage" 这种绝对路由——绝对路由对
                // 单纯注册的页面在 MAUI 里并不可靠，会出现「点了没反应」（既不导航也不抛异常）。
                var relativeRoute = route.StartsWith("//", StringComparison.Ordinal)
                    ? route.Substring(2)
                    : route;
                await shell.GoToAsync(relativeRoute);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"全局路由导航错误: {ex.Message}");
                throw;
            }
        }

        private async Task InitializeShellAndNavigateAsync(string route)
        {
            var appShell = _serviceProvider.GetRequiredService<AppShell>();

            Application.Current!.MainPage = appShell;

            await Task.Delay(250);

            await appShell.GoToAsync(route);
        }

    }
}
