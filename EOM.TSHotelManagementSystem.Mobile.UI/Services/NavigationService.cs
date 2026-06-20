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
                var pageType = ResolveRouteToPage(route);
                if (pageType == null)
                {
                    throw new InvalidOperationException($"无法解析路由: {route}");
                }

                if (IsPageCurrentlyDisplayed(pageType))
                {
                    return;
                }

                var page = _serviceProvider.GetService(pageType) as Page;

                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    await navPage.Navigation.PushAsync(page);
                }
                else if (Application.Current?.MainPage != null)
                {
                    Application.Current.MainPage = new NavigationPage(page);
                }
                else
                {
                    Application.Current!.MainPage = page;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"全局路由导航错误: {ex.Message}");
                throw;
            }
        }

        private bool IsPageCurrentlyDisplayed(Type pageType)
        {
            if (Application.Current?.MainPage is Page currentPage)
            {
                if (currentPage.GetType() == pageType)
                    return true;

                if (Application.Current.MainPage is NavigationPage navPage)
                {
                    return navPage.CurrentPage?.GetType() == pageType ||
                           navPage.Navigation.NavigationStack.LastOrDefault()?.GetType() == pageType;
                }
            }
            return false;
        }

        private async Task InitializeShellAndNavigateAsync(string route)
        {
            var appShell = _serviceProvider.GetRequiredService<AppShell>();

            Application.Current!.MainPage = appShell;

            await Task.Delay(250);

            await appShell.GoToAsync(route);
        }

        private Type ResolveRouteToPage(string route)
        {
            var routeMap = new Dictionary<string, Type>
            {
                { "//" + nameof(MainPage), typeof(MainPage) },
                { nameof(MainPage), typeof(MainPage) },
                { "//" + nameof(LoginPage), typeof(LoginPage) },
                { nameof(LoginPage), typeof(LoginPage) },
                { "//" + nameof(RegisterPage), typeof(RegisterPage) },
                { nameof(RegisterPage), typeof(RegisterPage) },
                { "//" + nameof(NewsView), typeof(NewsView) },
                { nameof(NewsView), typeof(NewsView) },
                { "//" + nameof(ProfileView), typeof(ProfileView) },
                { nameof(ProfileView), typeof(ProfileView) },
                { "//" + nameof(CheckInView), typeof(CheckInView) },
                { nameof(CheckInView), typeof(CheckInView) },
                { "//" + nameof(ReservationListView), typeof(ReservationListView) },
                { nameof(ReservationListView), typeof(ReservationListView) },
                { "//" + nameof(NewsDetailView), typeof(NewsDetailView) },
                { nameof(NewsDetailView), typeof(NewsDetailView) }
            };

            foreach (var mapping in routeMap)
            {
                if (route.StartsWith(mapping.Key, StringComparison.OrdinalIgnoreCase) ||
                    route.EndsWith(mapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.Value;
                }
            }

            var routeName = route.Split('/').LastOrDefault();
            if (!string.IsNullOrEmpty(routeName))
            {
                return routeMap.FirstOrDefault(k =>
                    k.Value.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase)).Value;
            }

            return null;
        }

    }
}
