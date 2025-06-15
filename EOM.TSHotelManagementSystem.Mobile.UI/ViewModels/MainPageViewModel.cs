using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private string _activeTab = "checkin";

        public MainPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public string ActiveTab
        {
            get => _activeTab;
            set => SetField(ref _activeTab, value);
        }
    }
}
