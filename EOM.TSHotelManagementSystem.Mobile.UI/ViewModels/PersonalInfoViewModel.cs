using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class PersonalInfoViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IProfileService _profileService;
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        private string _displayName;
        private string _account;
        private string _photoUrl;
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string _statusMessage;
        private string _statusMessageColor = "Green";
        private bool _isBusy;
        private bool _isChangingPassword;

        // 可编辑字段
        private string _editName;
        private string _editEmail;
        private string _editPhone;
        private string _editAddress;
        private DateTime? _editBirthDate;
        private int? _editGender;
        private int? _customerType;
        private string _selectedNationNumber;
        private string _selectedEducationNumber;

        // Picker 选中索引
        private int _selectedGenderIndex = -1;
        private int _selectedNationIndex = -1;
        private int _selectedEducationIndex = -1;
        private int _selectedCustomerTypeIndex = -1;

        public PersonalInfoViewModel(IProfileService profileService, IAuthService authService, INavigationService navigationService)
        {
            _profileService = profileService;
            _authService = authService;
            _navigationService = navigationService;
            LoadProfileCommand = new Command(async () => await LoadProfileAsync());
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            TogglePasswordSectionCommand = new Command(() => IsChangingPassword = !IsChangingPassword);
            SaveProfileCommand = new Command(async () => await SaveProfileAsync());
        }

        public string DisplayName { get => _displayName; set => SetField(ref _displayName, value); }
        public string Account { get => _account; set => SetField(ref _account, value); }
        public string PhotoUrl
        {
            get => _photoUrl;
            set
            {
                if (SetField(ref _photoUrl, value))
                {
                    OnPropertyChanged(nameof(HasPhoto));
                }
            }
        }
        public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoUrl);
        public string CurrentPassword { get => _currentPassword; set => SetField(ref _currentPassword, value); }
        public string NewPassword { get => _newPassword; set => SetField(ref _newPassword, value); }
        public string ConfirmPassword { get => _confirmPassword; set => SetField(ref _confirmPassword, value); }
        public string StatusMessage { get => _statusMessage; set => SetField(ref _statusMessage, value); }
        public string StatusMessageColor { get => _statusMessageColor; set => SetField(ref _statusMessageColor, value); }
        public bool IsBusy { get => _isBusy; set => SetField(ref _isBusy, value); }
        public bool IsChangingPassword { get => _isChangingPassword; set => SetField(ref _isChangingPassword, value); }

        // 可编辑属性
        public string EditName { get => _editName; set => SetField(ref _editName, value); }
        public string EditEmail { get => _editEmail; set => SetField(ref _editEmail, value); }
        public string EditPhone { get => _editPhone; set => SetField(ref _editPhone, value); }
        public string EditAddress { get => _editAddress; set => SetField(ref _editAddress, value); }
        public DateTime? EditBirthDate { get => _editBirthDate; set => SetField(ref _editBirthDate, value); }
        public int? EditGender { get => _editGender; set => SetField(ref _editGender, value); }
        public int? CustomerType { get => _customerType; set => SetField(ref _customerType, value); }

        // 数据源
        public ObservableCollection<string> GenderOptions { get; } = new() { "女", "男" };
        public ObservableCollection<string> CustomerTypeNames { get; } = new();
        public ObservableCollection<string> NationNames { get; } = new();
        public ObservableCollection<string> EducationNames { get; } = new();

        // 原始数据（用于编码映射）
        private List<CustoTypeOutputDto> _customerTypes = new();
        private List<NationOutputDto> _nations = new();
        private List<EducationOutputDto> _educations = new();

        // Picker 选中索引
        public int SelectedGenderIndex
        {
            get => _selectedGenderIndex;
            set
            {
                if (SetField(ref _selectedGenderIndex, value) && value >= 0)
                {
                    EditGender = value; // 0=女, 1=男
                }
            }
        }

        public int SelectedCustomerTypeIndex
        {
            get => _selectedCustomerTypeIndex;
            set
            {
                if (SetField(ref _selectedCustomerTypeIndex, value) && value >= 0 && value < _customerTypes.Count)
                {
                    CustomerType = _customerTypes[value].CustomerType;
                    OnPropertyChanged(nameof(CustomerTypeDisplay));
                }
            }
        }

        public int SelectedNationIndex
        {
            get => _selectedNationIndex;
            set
            {
                if (SetField(ref _selectedNationIndex, value) && value >= 0 && value < _nations.Count)
                {
                    _selectedNationNumber = _nations[value].NationNumber;
                }
            }
        }

        public int SelectedEducationIndex
        {
            get => _selectedEducationIndex;
            set
            {
                if (SetField(ref _selectedEducationIndex, value) && value >= 0 && value < _educations.Count)
                {
                    _selectedEducationNumber = _educations[value].EducationNumber;
                }
            }
        }

        public string CustomerTypeDisplay
        {
            get
            {
                if (_customerTypes.Any())
                {
                    var value = CustomerType.GetValueOrDefault();
                    var match = _customerTypes.FirstOrDefault(c => c.CustomerType == value);
                    return match?.CustomerTypeName ?? "普通客户";
                }
                return "普通客户";
            }
        }

        public ICommand LoadProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand TogglePasswordSectionCommand { get; }
        public ICommand SaveProfileCommand { get; }

        public async void OnViewAppearing()
        {
            await LoadProfileAsync();
        }

        public void OnViewDisappearing()
        {
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                IsBusy = true;

                // 并行加载数据源和个人信息
                var profileTask = _profileService.GetUserProfileAsync();
                var customerTypesTask = _profileService.GetCustomerTypesAsync();
                var nationsTask = _profileService.GetNationsAsync();
                var educationsTask = _profileService.GetEducationsAsync();

                await Task.WhenAll(profileTask, customerTypesTask, nationsTask, educationsTask);

                // 填充数据源
                _customerTypes = await customerTypesTask;
                _nations = await nationsTask;
                _educations = await educationsTask;

                CustomerTypeNames.Clear();
                foreach (var t in _customerTypes) CustomerTypeNames.Add(t.CustomerTypeName);

                NationNames.Clear();
                foreach (var n in _nations) NationNames.Add(n.NationName);

                EducationNames.Clear();
                foreach (var e in _educations) EducationNames.Add(e.EducationName);

                // 填充个人信息
                var profile = await profileTask;
                if (profile != null)
                {
                    DisplayName = profile.DisplayName ?? profile.Account;
                    Account = profile.UserNumber ?? profile.Account;
                    PhotoUrl = profile.PhotoUrl;

                    EditName = profile.DisplayName ?? string.Empty;
                    EditEmail = profile.EmailAddress ?? string.Empty;
                    EditPhone = profile.PhoneNumber ?? string.Empty;
                    EditAddress = profile.Address ?? string.Empty;
                    EditBirthDate = profile.DateOfBirth;
                    EditGender = profile.Gender;
                    CustomerType = profile.CustomerType;
                    _selectedNationNumber = profile.Ethnicity ?? string.Empty;
                    _selectedEducationNumber = profile.EducationLevel ?? string.Empty;

                    // 设置 Picker 选中项
                    SelectedGenderIndex = EditGender == 1 ? 1 : EditGender == 0 ? 0 : -1;
                    SelectedCustomerTypeIndex = _customerTypes.FindIndex(c => c.CustomerType == CustomerType);
                    SelectedNationIndex = _nations.FindIndex(n => n.NationNumber == _selectedNationNumber);
                    SelectedEducationIndex = _educations.FindIndex(e => e.EducationNumber == _selectedEducationNumber);

                    OnPropertyChanged(nameof(CustomerTypeDisplay));
                }
                else
                {
                    StatusMessage = "无法加载个人信息";
                    StatusMessageColor = "Red";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载资料失败: {ex.Message}";
                StatusMessageColor = "Red";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(EditName))
            {
                StatusMessage = "昵称不能为空";
                StatusMessageColor = "Red";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;

                var input = new UpdateProfileInputDto
                {
                    Name = EditName?.Trim(),
                    EmailAddress = EditEmail?.Trim(),
                    PhoneNumber = EditPhone?.Trim(),
                    Address = EditAddress?.Trim(),
                    DateOfBirth = EditBirthDate,
                    Gender = EditGender,
                    Ethnicity = _selectedNationNumber,
                    EducationLevel = _selectedEducationNumber
                };

                var success = await _profileService.UpdateProfileAsync(input);

                if (success)
                {
                    StatusMessage = "个人信息更新成功";
                    StatusMessageColor = "Green";
                    DisplayName = EditName;
                }
                else
                {
                    StatusMessage = "个人信息更新失败";
                    StatusMessageColor = "Red";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"更新失败: {ex.Message}";
                StatusMessageColor = "Red";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                StatusMessage = "请填写所有密码字段";
                StatusMessageColor = "Red";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                StatusMessage = "两次输入的新密码不一致";
                StatusMessageColor = "Red";
                return;
            }

            if (NewPassword == CurrentPassword)
            {
                StatusMessage = "新密码不能与旧密码相同";
                StatusMessageColor = "Red";
                return;
            }

            if (NewPassword.Length < 8)
            {
                StatusMessage = "新密码长度至少为8位";
                StatusMessageColor = "Red";
                return;
            }

            var hasLetter = Regex.IsMatch(NewPassword, "[a-zA-Z]");
            var hasDigit = Regex.IsMatch(NewPassword, @"\d");
            var hasSymbol = Regex.IsMatch(NewPassword, "[^a-zA-Z\\d]");
            if ((hasLetter ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSymbol ? 1 : 0) < 2)
            {
                StatusMessage = "密码必须包含字母、数字、标点符号中的至少两种";
                StatusMessageColor = "Red";
                return;
            }

            try
            {
                IsBusy = true;
                var httpService = MauiProgram.Services.GetService<IHttpService>();
                var json = HttpHelper.ModelToJson(new { OldPassword = CurrentPassword, NewPassword = NewPassword, ConfirmPassword = ConfirmPassword });
                var response = await httpService.RequestAsync("Profile/ChangePassword", json: json);

                if (string.IsNullOrWhiteSpace(response?.Message))
                {
                    StatusMessage = "服务器无响应";
                    StatusMessageColor = "Red";
                    return;
                }

                var result = HttpHelper.JsonToModel<ApiResponse>(response.Message);

                if (result?.Code == 0)
                {
                    await _authService.ClearTokenAsync();
                    await _authService.ClearBiometricCredentialsAsync();
                    await _navigationService.NavigateToAsync($"//{nameof(LoginPage)}");
                }
                else
                {
                    StatusMessage = result?.Message ?? "密码修改失败";
                    StatusMessageColor = "Red";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"密码修改失败: {ex.Message}";
                StatusMessageColor = "Red";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
