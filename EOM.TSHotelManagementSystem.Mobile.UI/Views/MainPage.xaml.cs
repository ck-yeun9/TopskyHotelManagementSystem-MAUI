using System.ComponentModel;

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

        _checkInView = new CheckInView();
        _newsView = new NewsView();
        _profileView = new ProfileView();

        SetActiveView(_viewModel.ActiveTab);
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
                    _newsView.BindingContext = _viewModel;
                    ContentHost.Content = _newsView;
                    break;

                case "checkin":
                    _checkInView.BindingContext = _viewModel;
                    ContentHost.Content = _checkInView;
                    break;

                case "profile":
                    _profileView.BindingContext = _viewModel;
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