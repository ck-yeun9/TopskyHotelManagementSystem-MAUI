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
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
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
            // 刷新当前标签页
            LoadTabContent(tabName);
        }
    }

    private void LoadTabContent(string tabName)
    {
        try
        {
            ContentView contentView = null;
            object bindingContext = null;

            switch (tabName)
            {
                case "news":
                    contentView = _serviceProvider.GetRequiredService<NewsView>();
                    bindingContext = _serviceProvider.GetRequiredService<NewsViewModel>();

                    // 设置页面标题
                    if (bindingContext is NewsViewModel newsViewModel)
                    {
                        newsViewModel.PageTitle = "新闻资讯";
                    }
                    break;

                case "checkin":
                    contentView = _serviceProvider.GetRequiredService<CheckInView>();
                    bindingContext = _viewModel; // 使用MainPageViewModel自身
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
            // 添加UI错误处理逻辑
        }
    }
}