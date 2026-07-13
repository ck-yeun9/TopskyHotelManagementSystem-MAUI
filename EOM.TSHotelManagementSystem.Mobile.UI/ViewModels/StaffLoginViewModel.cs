using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class StaffLoginViewModel : ViewModelBase
    {
        private readonly IStaffService _staffService;
        private readonly INavigationService _navigationService;

        public StaffLoginViewModel(IStaffService staffService, INavigationService navigationService)
        {
            _staffService = staffService;
            _navigationService = navigationService;

            LoginCommand = new Command(async () => await LoginAsync());
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetField(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetField(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetField(ref _isBusy, value);
        }

        public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

        public ICommand LoginCommand { get; }

        public void ResetState()
        {
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
            IsBusy = false;
        }

        private async Task LoginAsync()
        {
            IsBusy = true;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "用户名和密码不能为空";
                IsBusy = false;
                return;
            }

            var success = await _staffService.StaffLoginAsync(Username, Password);

            if (success)
            {
                await _navigationService.GoBackAsync();
            }
            else
            {
                ErrorMessage = "员工登录失败，请检查用户名和密码";
            }
            IsBusy = false;
        }
    }
}
