using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class ProfileViewModel : ViewModelBase, ILoadableViewModel
    {

        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetField(ref _userName, value);
        }

        private string _userLevel;
        public string UserLevel
        {
            get => _userLevel;
            set => SetField(ref _userLevel, value);
        }

        public ICommand NavigateToPersonalCenterCommand { get; private set; }

        public async void OnViewAppearing()
        {
            await LoadUserDataAsync();
        }

        private async Task LoadUserDataAsync()
        {
            UserName = "Jackson";
            UserLevel = "钻石会员";
        }

        public ProfileViewModel()
        {
            NavigateToPersonalCenterCommand = new Command(NavigateToPersonalCenter);
        }

        private void NavigateToPersonalCenter()
        {
        }

        public void OnViewDisappearing()
        {
        }
    }
}