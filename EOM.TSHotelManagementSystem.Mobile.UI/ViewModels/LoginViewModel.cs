using EOM.TSHotelManagementSystem.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;

            LoginCommand = new Command(async () => await LoginAsync());
            NavigateToRegisterCommand = new Command(NavigateToRegister);
            SkipLoginCommand = new Command(async () => await SkipLoginAsync());
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

        private bool _isbusy;
        public bool IsBusy
        {
            get => _isbusy;
            set => SetField(ref _isbusy, value);
        }

        public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand SkipLoginCommand { get; }

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

            var success = await _authService.LoginAsync(Username, Password);

            if (success)
            {
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                ErrorMessage = "登录失败，请检查用户名和密码";
            }
            IsBusy = false;
        }

        private async Task SkipLoginAsync()
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }

        private async void NavigateToRegister()
        {
            try
            {
                await _navigationService.NavigateToAsync($"//{nameof(RegisterPage)}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"无法打开注册页面: {ex.Message}";
            }
        }
    }
}
