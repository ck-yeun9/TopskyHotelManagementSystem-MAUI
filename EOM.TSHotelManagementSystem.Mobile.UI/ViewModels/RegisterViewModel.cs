using EOM.TSHotelManagementSystem.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private bool _isbusy;
        public bool IsBusy
        {
            get => _isbusy;
            set => SetField(ref _isbusy, value);
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
            IsBusy = false;
            ErrorMessage = string.Empty;
        }

        private async Task RegisterAsync()
        {
            IsBusy = true;

            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "所有字段都必须填写";
                IsBusy = false;
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "两次输入的密码不一致";
                IsBusy = false;
                return;
            }

            if (Password.Length < 8)
            {
                ErrorMessage = "密码长度至少为8位";
                IsBusy = false;
                return;
            }

            var hasLetter = Regex.IsMatch(Password, "[a-zA-Z]");
            var hasDigit = Regex.IsMatch(Password, @"\d");
            var hasSymbol = Regex.IsMatch(Password, "[^a-zA-Z\\d]");
            if ((hasLetter ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSymbol ? 1 : 0) < 2)
            {
                ErrorMessage = "密码必须包含字母、数字、标点符号中的至少两种";
                IsBusy = false;
                return;
            }

            try
            {
                var success = await _authService.RegisterAsync(Username, Email, Password);

                if (success)
                {
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                }
                else
                {
                    ErrorMessage = "注册失败，请重试";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void NavigateToLogin()
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }
}
