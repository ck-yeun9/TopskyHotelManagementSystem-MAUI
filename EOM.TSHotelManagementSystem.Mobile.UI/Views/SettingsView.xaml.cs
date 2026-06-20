namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class SettingsView : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Shell.Current.Title = "系统设置";
        await _viewModel.LoadSettingsAsync();
    }
}
