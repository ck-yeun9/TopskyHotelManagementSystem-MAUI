using EOM.TSHotelManagementSystem.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        public RegisterViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;

            RegisterCommand = new Command(async () => await RegisterAsync());
            NavigateToLoginCommand = new Command(NavigateToLogin);
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetField(ref _username, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetField(ref _confirmPassword, value);
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

        public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public void ResetState()
        {
            Username = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            ErrorMessage = string.Empty;
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "所有字段都必须填写";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "两次输入的密码不一致";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "密码长度至少为6位";
                return;
            }

            var success = await _authService.RegisterAsync(Username, Email, Password);

            if (success)
            {
                await _navigationService.NavigateToAsync(nameof(MainPage));
            }
            else
            {
                ErrorMessage = "注册失败，请重试";
            }
        }

        private void NavigateToLogin()
        {
            _navigationService.NavigateToAsync(nameof(LoginPage));
        }
    }
}
