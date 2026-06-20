using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class CheckInViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IBookingService _bookingService;
        private readonly IAuthService _authService;
        private bool _hasLoadedRooms;
        private string _activeTab = "booking";
        private DateTime _checkInDate = DateTime.Today.AddDays(1);
        private DateTime _checkOutDate = DateTime.Today.AddDays(2);
        private int _guestCount = 2;
        private AvailableRoomDto? _selectedRoom;
        private string _contactName = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _specialRequest = string.Empty;
        private bool _isLoadingRooms;
        private bool _isSubmitting;
        private string _statusMessage = string.Empty;
        private Color _statusMessageColor = Colors.Transparent;
        private CreateReservationOutputDto? _latestReservation;

        public CheckInViewModel(IBookingService bookingService, IAuthService authService)
        {
            _bookingService = bookingService;
            _authService = authService;

            AvailableRooms = new ObservableCollection<AvailableRoomDto>();
            CurrentCheckins = new ObservableCollection<CurrentCheckinDto>();
            ConsumptionHistory = new ObservableCollection<ConsumptionRecordDto>();
            GuestCountOptions = new List<int> { 1, 2, 3, 4, 5, 6 };
            RefreshAvailableRoomsCommand = new Command(async () => await LoadRoomDataAsync());
            SubmitReservationCommand = new Command(async () => await SubmitReservationAsync());
            SwitchTabCommand = new Command<string>(async (tab) => await SwitchTabAsync(tab));
        }

        public ObservableCollection<AvailableRoomDto> AvailableRooms { get; }
        public ObservableCollection<CurrentCheckinDto> CurrentCheckins { get; }
        public ObservableCollection<ConsumptionRecordDto> ConsumptionHistory { get; }
        public IReadOnlyList<int> GuestCountOptions { get; }

        public ICommand RefreshAvailableRoomsCommand { get; }
        public ICommand SubmitReservationCommand { get; }
        public ICommand SwitchTabCommand { get; }

        public string ActiveTab
        {
            get => _activeTab;
            set
            {
                if (SetField(ref _activeTab, value))
                {
                    OnPropertyChanged(nameof(IsBookingTab));
                    OnPropertyChanged(nameof(IsCheckinTab));
                    OnPropertyChanged(nameof(IsHistoryTab));
                    OnPropertyChanged(nameof(BookingTabColor));
                    OnPropertyChanged(nameof(BookingTabTextColor));
                    OnPropertyChanged(nameof(CheckinTabColor));
                    OnPropertyChanged(nameof(CheckinTabTextColor));
                    OnPropertyChanged(nameof(HistoryTabColor));
                    OnPropertyChanged(nameof(HistoryTabTextColor));
                }
            }
        }

        public bool IsBookingTab => ActiveTab == "booking";
        public bool IsCheckinTab => ActiveTab == "checkin";
        public bool IsHistoryTab => ActiveTab == "history";

        public Color BookingTabColor => ActiveTab == "booking" ? Color.FromArgb("#512BD4") : Color.FromArgb("#F3F4F6");
        public Color BookingTabTextColor => ActiveTab == "booking" ? Colors.White : Color.FromArgb("#6B7280");
        public Color CheckinTabColor => ActiveTab == "checkin" ? Color.FromArgb("#512BD4") : Color.FromArgb("#F3F4F6");
        public Color CheckinTabTextColor => ActiveTab == "checkin" ? Colors.White : Color.FromArgb("#6B7280");
        public Color HistoryTabColor => ActiveTab == "history" ? Color.FromArgb("#512BD4") : Color.FromArgb("#F3F4F6");
        public Color HistoryTabTextColor => ActiveTab == "history" ? Colors.White : Color.FromArgb("#6B7280");

        public string CurrentCheckinMessage => CurrentCheckins.Count > 0
            ? $"您当前有 {CurrentCheckins.Count} 个正在入住的房间"
            : "您当前没有正在入住的房间";

        public DateTime Today => DateTime.Today;

        public DateTime CheckInDate
        {
            get => _checkInDate;
            set
            {
                var normalized = value.Date;
                if (SetField(ref _checkInDate, normalized))
                {
                    if (CheckOutDate <= _checkInDate)
                    {
                        CheckOutDate = _checkInDate.AddDays(1);
                    }
                    NotifyReservationSummaryChanged();
                }
            }
        }

        public DateTime CheckOutDate
        {
            get => _checkOutDate;
            set
            {
                var normalized = value.Date <= _checkInDate.Date ? _checkInDate.Date.AddDays(1) : value.Date;
                if (SetField(ref _checkOutDate, normalized))
                {
                    NotifyReservationSummaryChanged();
                }
            }
        }

        public DateTime MinCheckOutDate => CheckInDate.AddDays(1);

        public int GuestCount
        {
            get => _guestCount;
            set
            {
                if (SetField(ref _guestCount, value))
                {
                    NotifyReservationSummaryChanged();
                }
            }
        }

        public AvailableRoomDto? SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                if (SetField(ref _selectedRoom, value))
                {
                    NotifyReservationSummaryChanged();
                }
            }
        }

        public string ContactName
        {
            get => _contactName;
            set => SetField(ref _contactName, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetField(ref _phoneNumber, value);
        }

        public string SpecialRequest
        {
            get => _specialRequest;
            set => SetField(ref _specialRequest, value);
        }

        public bool IsLoadingRooms
        {
            get => _isLoadingRooms;
            set => SetField(ref _isLoadingRooms, value);
        }

        public bool IsSubmitting
        {
            get => _isSubmitting;
            set => SetField(ref _isSubmitting, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (SetField(ref _statusMessage, value))
                {
                    OnPropertyChanged(nameof(HasStatusMessage));
                }
            }
        }

        public Color StatusMessageColor
        {
            get => _statusMessageColor;
            set => SetField(ref _statusMessageColor, value);
        }

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        public CreateReservationOutputDto? LatestReservation
        {
            get => _latestReservation;
            set
            {
                if (SetField(ref _latestReservation, value))
                {
                    OnPropertyChanged(nameof(HasLatestReservation));
                }
            }
        }

        public bool HasLatestReservation => LatestReservation is not null;

        public int NightCount => Math.Max(1, (CheckOutDate.Date - CheckInDate.Date).Days);

        public decimal EstimatedTotal => SelectedRoom is null ? 0 : SelectedRoom.PricePerNight * NightCount;

        public string SelectedRoomSummary =>
            SelectedRoom is null
                ? "请先选择房间类型。"
                : $"{SelectedRoom.RoomName} | 最多{SelectedRoom.MaxGuests}人";

        public string StaySummary => $"{CheckInDate:yyyy-MM-dd} 至 {CheckOutDate:yyyy-MM-dd} | {NightCount}天";

        public string PriceSummary =>
            SelectedRoom is null
                ? "选择房间后将显示预估总价。"
                : $"预估总价: ¥{EstimatedTotal:F0}";

        public async void OnViewAppearing()
        {
            if (!_hasLoadedRooms)
            {
                await LoadRoomDataAsync();
            }
        }

        public void OnViewDisappearing()
        {
        }

        private async Task SwitchTabAsync(string tab)
        {
            ActiveTab = tab;

            if (tab == "checkin" && CurrentCheckins.Count == 0)
            {
                await LoadCurrentCheckinsAsync();
            }
            else if (tab == "history" && ConsumptionHistory.Count == 0)
            {
                await LoadConsumptionHistoryAsync();
            }
        }

        private async Task LoadRoomDataAsync()
        {
            if (CheckOutDate <= CheckInDate)
            {
                StatusMessage = "离店日期必须晚于入住日期。";
                StatusMessageColor = Colors.OrangeRed;
                return;
            }

            try
            {
                IsLoadingRooms = true;
                StatusMessage = string.Empty;
                LatestReservation = null;

                var rooms = await _bookingService.GetAvailableRoomsAsync(CheckInDate, CheckOutDate, GuestCount);
                var selectedRoomId = SelectedRoom?.RoomTypeId;

                AvailableRooms.Clear();
                foreach (var room in rooms)
                {
                    AvailableRooms.Add(room);
                }

                SelectedRoom = AvailableRooms.FirstOrDefault(room => room.RoomTypeId == selectedRoomId)
                    ?? AvailableRooms.FirstOrDefault();

                _hasLoadedRooms = true;

                if (AvailableRooms.Count == 0)
                {
                    StatusMessage = "当前条件下没有可用房间，请尝试其他日期或人数。";
                    StatusMessageColor = Colors.OrangeRed;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载房间失败: {ex.Message}";
                StatusMessageColor = Colors.OrangeRed;
            }
            finally
            {
                IsLoadingRooms = false;
                NotifyReservationSummaryChanged();
            }
        }

        private async Task LoadCurrentCheckinsAsync()
        {
            try
            {
                var isLoggedIn = await _authService.HasValidTokenAsync();
                if (!isLoggedIn)
                {
                    CurrentCheckins.Clear();
                    return;
                }

                var httpService = MauiProgram.Services.GetService<IHttpService>();
                var response = await httpService.RequestAsync("MobileBooking/GetCurrentCheckins");

                if (string.IsNullOrWhiteSpace(response?.Message))
                    return;

                var result = HttpHelper.JsonToModel<ApiResponse<List<CurrentCheckinDto>>>(response.Message);
                if (result?.Code == 0 && result.Data != null)
                {
                    CurrentCheckins.Clear();
                    foreach (var item in result.Data)
                    {
                        CurrentCheckins.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCurrentCheckinsAsync Exception: {ex.Message}");
            }
        }

        private async Task LoadConsumptionHistoryAsync()
        {
            try
            {
                var isLoggedIn = await _authService.HasValidTokenAsync();
                if (!isLoggedIn)
                {
                    ConsumptionHistory.Clear();
                    return;
                }

                var httpService = MauiProgram.Services.GetService<IHttpService>();
                var response = await httpService.RequestAsync("MobileBooking/GetConsumptionHistory");

                if (string.IsNullOrWhiteSpace(response?.Message))
                    return;

                var result = HttpHelper.JsonToModel<ApiResponse<List<ConsumptionRecordDto>>>(response.Message);
                if (result?.Code == 0 && result.Data != null)
                {
                    ConsumptionHistory.Clear();
                    foreach (var item in result.Data)
                    {
                        ConsumptionHistory.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadConsumptionHistoryAsync Exception: {ex.Message}");
            }
        }

        private async Task SubmitReservationAsync()
        {
            if (!await _authService.HasValidTokenAsync())
            {
                StatusMessage = "请先登录后再进行预约。";
                StatusMessageColor = Colors.OrangeRed;
                await Shell.Current.GoToAsync(nameof(LoginPage));
                return;
            }

            var validationMessage = ValidateForm();
            if (!string.IsNullOrEmpty(validationMessage))
            {
                StatusMessage = validationMessage;
                StatusMessageColor = Colors.OrangeRed;
                return;
            }

            try
            {
                IsSubmitting = true;
                StatusMessage = string.Empty;

                var result = await _bookingService.CreateReservationAsync(new CreateReservationInputDto
                {
                    RoomTypeId = SelectedRoom!.RoomTypeId,
                    RoomName = SelectedRoom.RoomName,
                    CheckInDate = CheckInDate,
                    CheckOutDate = CheckOutDate,
                    GuestCount = GuestCount,
                    ContactName = ContactName.Trim(),
                    PhoneNumber = PhoneNumber.Trim(),
                    SpecialRequest = SpecialRequest.Trim()
                });

                LatestReservation = result;
                StatusMessage = $"预订成功。预订号: {result.ReservationNumber}。";
                StatusMessageColor = Colors.Green;
            }
            catch (Exception ex)
            {
                StatusMessage = $"预订失败: {ex.Message}";
                StatusMessageColor = Colors.OrangeRed;
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private string ValidateForm()
        {
            if (SelectedRoom is null)
            {
                return "请先选择房间类型。";
            }

            if (CheckOutDate <= CheckInDate)
            {
                return "离店日期必须晚于入住日期。";
            }

            if (string.IsNullOrWhiteSpace(ContactName))
            {
                return "请输入联系人姓名。";
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                return "请输入手机号码。";
            }

            var digits = Regex.Replace(PhoneNumber, "[^0-9]", string.Empty);
            if (digits.Length < 6)
            {
                return "手机号码格式不正确。";
            }

            if (GuestCount > SelectedRoom.MaxGuests)
            {
                return "所选房间无法容纳当前人数。";
            }

            return string.Empty;
        }

        private void NotifyReservationSummaryChanged()
        {
            OnPropertyChanged(nameof(MinCheckOutDate));
            OnPropertyChanged(nameof(NightCount));
            OnPropertyChanged(nameof(EstimatedTotal));
            OnPropertyChanged(nameof(SelectedRoomSummary));
            OnPropertyChanged(nameof(StaySummary));
            OnPropertyChanged(nameof(PriceSummary));
        }
    }

    public class CurrentCheckinDto
    {
        public string RoomName { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckInTime { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }

    public class ConsumptionRecordDto
    {
        public string RoomName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public string DateRange => $"{CheckInDate:yyyy-MM-dd} 至 {CheckOutDate:yyyy-MM-dd}";
    }
}
