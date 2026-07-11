using Microsoft.Maui.Controls;

namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is RegisterViewModel vm)
        {
            vm.ResetState();
        }
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        var border = sender is Entry entry
            && entry.Parent is Grid grid
            && grid.Parent is Border b
            ? b : null;
        if (border != null)
            border.Stroke = new SolidColorBrush(Color.FromArgb("#FF5722"));
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        var border = sender is Entry entry
            && entry.Parent is Grid grid
            && grid.Parent is Border b
            ? b : null;
        if (border != null)
            border.Stroke = Application.Current.RequestedTheme == AppTheme.Dark
                ? new SolidColorBrush(Color.FromArgb("#3A3A3C"))
                : new SolidColorBrush(Color.FromArgb("#E0E0E0"));
    }
}