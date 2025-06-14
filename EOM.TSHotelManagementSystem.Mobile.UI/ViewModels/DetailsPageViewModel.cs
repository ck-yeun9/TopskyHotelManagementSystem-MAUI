using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class DetailsPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public DetailsPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            GoBackCommand = new Command(GoBack);
        }

        public Command GoBackCommand { get; }

        private async void GoBack()
        {
            await _navigationService.GoBackAsync();
        }
    }
}
