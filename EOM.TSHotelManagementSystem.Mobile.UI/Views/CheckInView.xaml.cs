using EOM.TSHotelManagementSystem.Mobile.Contract;

namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class CheckInView : ContentView
{
    public CheckInView()
    {
        InitializeComponent();
    }

    private void OnRoomTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is AvailableRoomDto room)
        {
            if (BindingContext is CheckInViewModel viewModel)
            {
                viewModel.SelectedRoom = room;
            }
        }
    }
}