using System.ComponentModel;
using System.Diagnostics;

namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public MainPage(
        MainPageViewModel viewModel,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _serviceProvider = serviceProvider;

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        LoadTabContent(_viewModel.ActiveTab);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BottomNavBar != null)
        {
            BottomNavBar.UpdateActiveTab(_viewModel.ActiveTab);
        }
        LoadTabContent(_viewModel.ActiveTab);
        UpdateShellTitle();
    }

    private void UpdateShellTitle()
    {
        var title = _viewModel.ActiveTab switch
        {
            "news" => "最新资讯",
            "checkin" => "入住管理",
            "profile" => "个人中心",
            _ => "TopSky酒店"
        };
        Shell.Current.Title = title;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainPageViewModel.ActiveTab))
        {
            LoadTabContent(_viewModel.ActiveTab);
            if (BottomNavBar != null)
            {
                BottomNavBar.UpdateActiveTab(_viewModel.ActiveTab);
            }
        }
    }

    private void OnTabSelected(object sender, string tabName)
    {
        if (_viewModel.ActiveTab != tabName)
        {
            _viewModel.ActiveTab = tabName;
        }
        else
        {
            LoadTabContent(tabName);
        }
    }

    private void LoadTabContent(string tabName)
    {
        try
        {
            if (ContentHost.Content?.BindingContext is ILoadableViewModel currentLoadable)
            {
                currentLoadable.OnViewDisappearing();
            }

            ContentView? contentView = null;
            object? bindingContext = null;

            switch (tabName)
            {
                case "news":
                    contentView = _serviceProvider.GetRequiredService<NewsView>();
                    bindingContext = _serviceProvider.GetRequiredService<NewsViewModel>();

                    if (bindingContext is NewsViewModel newsViewModel)
                    {
                        newsViewModel.PageTitle = "新闻资讯";
                    }
                    break;

                case "checkin":
                    contentView = _serviceProvider.GetRequiredService<CheckInView>();
                    bindingContext = _serviceProvider.GetRequiredService<CheckInViewModel>();
                    break;

                case "profile":
                    contentView = _serviceProvider.GetRequiredService<ProfileView>();
                    bindingContext = _serviceProvider.GetRequiredService<ProfileViewModel>();
                    break;
            }

            if (contentView != null)
            {
                contentView.BindingContext = bindingContext;
                ContentHost.Content = contentView;

                if (bindingContext is ILoadableViewModel loadable)
                {
                    loadable.OnViewAppearing();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"加载标签页错误: {ex.Message}");
        }
    }
}
