using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private CheckInView _checkInView;
    private NewsView _newsView;
    private ProfileView _profileView;

    public MainPage(MainPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = _viewModel = viewModel;
        Appearing += OnMainPageAppearing;
        Title = "TopSky¾Æµê";

        InitializeViews();

        SetActiveView(_viewModel.ActiveTab);
    }

    private void InitializeViews()
    {
        _checkInView = App.Services.GetService<CheckInView>();
        _newsView = App.Services.GetService<NewsView>();
        _profileView = App.Services.GetService<ProfileView>();

        _checkInView.BindingContext = _viewModel;
        _profileView.BindingContext = _viewModel;

        _newsView.BindingContext = App.Services.GetService<NewsViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
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
            SetActiveView(_viewModel.ActiveTab);
        }
    }

    private void SetActiveView(string tabName)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (tabName)
            {
                case "news":
                    ContentHost.Content = _newsView;
                    break;

                case "checkin":
                    ContentHost.Content = _checkInView;
                    break;

                case "profile":
                    ContentHost.Content = _profileView;
                    break;
            }

            BottomNavBar?.UpdateActiveTab(tabName);
        });
    }

    private void OnMainPageAppearing(object sender, EventArgs e)
    {
        BottomNavBar.UpdateActiveTab(_viewModel.ActiveTab);

        if (BindingContext is MainPageViewModel viewModel && BottomNavBar != null)
        {
            BottomNavBar.UpdateActiveTab(viewModel.ActiveTab);
        }
    }
}