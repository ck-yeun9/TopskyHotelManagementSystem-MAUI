namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class ProfileView : ContentView
{
    public ProfileView(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
