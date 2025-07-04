using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class CheckInViewModel : ViewModelBase, ILoadableViewModel
    {
        public async void OnViewAppearing()
        {
            await LoadRoomDataAsync();
        }

        public void OnViewDisappearing()
        {

        }

        private async Task LoadRoomDataAsync()
        {
            await Task.Delay(300); 
        }
    }
}
