using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class CheckInViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IBookingService _bookingService;
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private bool _hasLoadedRooms;
        private bool _hasLoadedCheckins;
        private bool _hasLoadedHistory;
        // 结算成功后由 OrderCheckoutViewModel 置位：返回入住页（复用既有 VM 实例）时
        // 强制刷新在住房间列表，使房间「商品消费/当前消费」总额更新为最新，避免显示脏数据。
        public static bool NeedsCheckinRefresh { get; set; }
        private string _activeTab = "booking";
        private CurrentCheckinDto? _selectedCheckin;
        private int _historyPage = 1;
        private int _historyPageSize = 10;
        private bool _hasMoreHistory = true;
        private bool _isLoadingMoreHistory;
        private DateTime _checkInDate = DateTime.Today.AddDays(1);
        private DateTime _checkOutDate = DateTime.Today.AddDays(2);
        private int _guestCount = 2;
        private AvailableRoomDto? _selectedRoom;
        private string _contactName = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _specialRequest = string.Empty;
        private bool _isLoadingRooms;
        private bool _isLoadingCheckins;
        private bool _isLoadingHistory;
        private bool _isRefreshing;
        private bool _isSubmitting;
        private bool _isNavigatingToShop;
        private string _statusMessage = string.Empty;
        private Color _statusMessageColor = Colors.Transparent;
        private CreateReservationOutputDto? _latestReservation;

        public CheckInViewModel(IBookingService bookingService, IAuthService authService, INavigationService navigationService)
        {
            _bookingService = bookingService;
            _authService = authService;
            _navigationService = navigationService;

            AvailableRooms = new ObservableCollection<AvailableRoomDto>();
            CurrentCheckins = new ObservableCollection<CurrentCheckinDto>();
            ConsumptionHistory = new ObservableCollection<ConsumptionRecordDto>();
            GroupedConsumptionHistory = new ObservableCollection<ConsumptionDateGroup>();
            GuestCountOptions = new List<int> { 1, 2, 3, 4, 5, 6 };
            RefreshAvailableRoomsCommand = new Command(async () => await LoadRoomDataAsync());
            SubmitReservationCommand = new Command(async () => await SubmitReservationAsync());
            SwitchTabCommand = new Command<string>(async (tab) => await SwitchTabAsync(tab));
            SelectCheckinCommand = new Command<CurrentCheckinDto>(OnSelectCheckin);
            ViewRoomDetailCommand = new Command(async () => await OnViewRoomDetail());
            ConsumeProductCommand = new Command(async () => await OnConsumeProduct());
            ViewHistoryReviewCommand = new Command(async () => await OnViewHistoryReview());
            LoadMoreHistoryCommand = new Command(async () => await LoadMoreHistoryAsync());
            ChangeHistoryRangeCommand = new Command<string>(OnChangeHistoryRange);
            RefreshCommand = new Command(async () => await RefreshAsync());
        }

        public ObservableCollection<AvailableRoomDto> AvailableRooms { get; }
        public ObservableCollection<CurrentCheckinDto> CurrentCheckins { get; }
        public ObservableCollection<ConsumptionRecordDto> ConsumptionHistory { get; }
        public ObservableCollection<ConsumptionDateGroup> GroupedConsumptionHistory { get; }
        public IReadOnlyList<int> GuestCountOptions { get; }

        public ICommand RefreshAvailableRoomsCommand { get; }
        public ICommand SubmitReservationCommand { get; }
        public ICommand SwitchTabCommand { get; }
        public ICommand SelectCheckinCommand { get; }
        public ICommand ViewRoomDetailCommand { get; }
        public ICommand ConsumeProductCommand { get; }
        public ICommand ViewHistoryReviewCommand { get; }
        public ICommand LoadMoreHistoryCommand { get; }
        public ICommand ChangeHistoryRangeCommand { get; }
        public ICommand RefreshCommand { get; }

        private string _historyRange = "1m";
        /// <summary>
        /// 消费记录时间范围：all=全部，1m=近一月，3m=近三月，1y=近一年。默认近一月。
        /// 切换时重新按范围加载，并重建按日期分组。
        /// </summary>
        public string HistoryRange
        {
            get => _historyRange;
            set
            {
                if (SetField(ref _historyRange, value))
                {
                    _ = LoadConsumptionHistoryAsync();
                }
            }
        }

        public CurrentCheckinDto? SelectedCheckin
        {
            get => _selectedCheckin;
            set
            {
                if (SetField(ref _selectedCheckin, value))
                {
                    OnPropertyChanged(nameof(HasSelectedCheckin));
                }
            }
        }

        public bool HasSelectedCheckin => SelectedCheckin is not null;

        public bool HasMoreHistory
        {
            get => _hasMoreHistory;
            set => SetField(ref _hasMoreHistory, value);
        }

        public bool IsLoadingMoreHistory
        {
            get => _isLoadingMoreHistory;
            set => SetField(ref _isLoadingMoreHistory, value);
        }

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

        public Color BookingTabColor => ActiveTab == "booking" ? Color.FromArgb("#FF5722") : Color.FromArgb("#F3F4F6");
        public Color BookingTabTextColor => ActiveTab == "booking" ? Colors.White : Color.FromArgb("#6B7280");
        public Color CheckinTabColor => ActiveTab == "checkin" ? Color.FromArgb("#FF5722") : Color.FromArgb("#F3F4F6");
        public Color CheckinTabTextColor => ActiveTab == "checkin" ? Colors.White : Color.FromArgb("#6B7280");
        public Color HistoryTabColor => ActiveTab == "history" ? Color.FromArgb("#FF5722") : Color.FromArgb("#F3F4F6");
        public Color HistoryTabTextColor => ActiveTab == "history" ? Colors.White : Color.FromArgb("#6B7280");

        public string CurrentCheckinMessage => CurrentCheckins.Count > 0
            ? $"您当前有 {CurrentCheckins.Count} 个正在入住的房间"
            : "您当前没有正在入住的房间";

        public bool HasNoCheckins => CurrentCheckins.Count == 0;
        public bool HasNoConsumptionHistory => ConsumptionHistory.Count == 0;

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

        public bool IsLoadingCheckins
        {
            get => _isLoadingCheckins;
            set => SetField(ref _isLoadingCheckins, value);
        }

        public bool IsLoadingHistory
        {
            get => _isLoadingHistory;
            set => SetField(ref _isLoadingHistory, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetField(ref _isRefreshing, value);
        }

        public bool IsSubmitting
        {
            get => _isSubmitting;
            set => SetField(ref _isSubmitting, value);
        }

        public bool IsNavigatingToShop
        {
            get => _isNavigatingToShop;
            set => SetField(ref _isNavigatingToShop, value);
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

            // 仅当刚下过单时才刷新在住房间列表（复用既有 VM 实例不会自动重载）。
            // 平时返回本页不触发，避免每次返回都重新请求造成卡顿。
            if (NeedsCheckinRefresh)
            {
                NeedsCheckinRefresh = false;
                _hasLoadedCheckins = false;
                await LoadCurrentCheckinsAsync();

                // 重新指向同一房间，避免选中项停留在被清空的旧对象上（旧对象消费额是脏数据）
                if (SelectedCheckin != null)
                {
                    var fresh = CurrentCheckins.FirstOrDefault(c => c.RoomNumber == SelectedCheckin.RoomNumber);
                    if (fresh != null)
                    {
                        foreach (var item in CurrentCheckins) item.IsSelected = false;
                        fresh.IsSelected = true;
                        SelectedCheckin = fresh;
                        OnPropertyChanged(nameof(HasSelectedCheckin));
                    }
                }
            }
        }

        public void OnViewDisappearing()
        {
        }

        private void OnSelectCheckin(CurrentCheckinDto checkin)
        {
            foreach (var item in CurrentCheckins)
            {
                item.IsSelected = false;
            }

            if (SelectedCheckin == checkin)
            {
                SelectedCheckin = null;
            }
            else
            {
                checkin.IsSelected = true;
                SelectedCheckin = checkin;
            }

            OnPropertyChanged(nameof(HasSelectedCheckin));
        }

        private async Task OnViewRoomDetail()
        {
            if (SelectedCheckin == null) return;
            var detail = $"房号: {SelectedCheckin.RoomNumber}";
            if (!string.IsNullOrWhiteSpace(SelectedCheckin.RoomArea))
                detail += $"\n区域: {SelectedCheckin.RoomArea}";
            if (SelectedCheckin.RoomFloor.HasValue)
                detail += $"\n楼层: {SelectedCheckin.RoomFloor}F";
            detail += $"\n房型: {SelectedCheckin.RoomName}" +
                $"\n入住时间: {SelectedCheckin.CheckInTime:yyyy-MM-dd HH:mm}" +
                $"\n房费: ¥{SelectedCheckin.DailyRate:F0}/天" +
                $"\n客户类型: {SelectedCheckin.CustomerLevelName} ({SelectedCheckin.DiscountDisplay})" +
                $"\n折扣后房费: ¥{SelectedCheckin.DiscountedDailyRate:F0}/天" +
                $"\n商品消费: ¥{SelectedCheckin.ProductConsumption:F0}" +
                $"\n当前消费: ¥{SelectedCheckin.TotalAmount:F0}";
            await Shell.Current.DisplayAlertAsync("房间详情", detail, "确定");
        }

        private async Task OnConsumeProduct()
        {
            if (SelectedCheckin == null) return;
            try
            {
                IsNavigatingToShop = true;
                var vm = MauiProgram.Services.GetService<ProductShopViewModel>();
                vm.Initialize(SelectedCheckin.RoomNumber);
                var page = new ProductShopView(vm);
                await Application.Current.Windows[0].Page.Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnConsumeProduct Exception: {ex}");
                await Shell.Current.DisplayAlertAsync("错误", ex.Message, "确定");
            }
            finally
            {
                IsNavigatingToShop = false;
            }
        }

        private async Task OnViewHistoryReview()
        {
            if (SelectedCheckin == null) return;
            try
            {
                var page = MauiProgram.Services.GetRequiredService<EvaluationStatsView>();
                page.Initialize(SelectedCheckin.RoomNumber, SelectedCheckin.RoomName, SelectedCheckin.StayId);
                await Shell.Current.Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnViewHistoryReview Exception: {ex}");
                await Shell.Current.DisplayAlertAsync("错误", ex.Message, "确定");
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                if (ActiveTab == "checkin")
                {
                    _hasLoadedCheckins = false;
                    await LoadCurrentCheckinsAsync();
                }
                else if (ActiveTab == "history")
                {
                    _hasLoadedHistory = false;
                    await LoadConsumptionHistoryAsync();
                }
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task SwitchTabAsync(string tab)
        {
            System.Diagnostics.Debug.WriteLine($"SwitchTabAsync called with tab={tab}, _hasLoadedHistory={_hasLoadedHistory}");
            ActiveTab = tab;

            try
            {
                if (tab == "checkin" && !_hasLoadedCheckins)
                {
                    await LoadCurrentCheckinsAsync();
                }
                else if (tab == "history" && !_hasLoadedHistory)
                {
                    System.Diagnostics.Debug.WriteLine("SwitchTabAsync: calling LoadConsumptionHistoryAsync");
                    await LoadConsumptionHistoryAsync();
                    System.Diagnostics.Debug.WriteLine($"SwitchTabAsync: LoadConsumptionHistoryAsync done, count={ConsumptionHistory.Count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SwitchTabAsync Exception: {ex}");
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
                IsLoadingCheckins = true;

                var isLoggedIn = await _authService.HasValidTokenAsync();
                if (!isLoggedIn)
                {
                    CurrentCheckins.Clear();
                    OnPropertyChanged(nameof(HasNoCheckins));
                    OnPropertyChanged(nameof(CurrentCheckinMessage));
                    System.Diagnostics.Debug.WriteLine("LoadCurrentCheckinsAsync: Not logged in");
                    return;
                }

                var httpService = MauiProgram.Services.GetService<IHttpService>();
                var response = await httpService.RequestAsync("MobileBooking/GetCurrentCheckins");

                System.Diagnostics.Debug.WriteLine($"LoadCurrentCheckinsAsync Response: {response?.Message}");

                if (string.IsNullOrWhiteSpace(response?.Message))
                    return;

                var result = HttpHelper.JsonToModel<ApiResponse<PagedData<CurrentCheckinDto>>>(response.Message);
                System.Diagnostics.Debug.WriteLine($"LoadCurrentCheckinsAsync Result: Code={result?.Code}, Data?.Items?.Count={result?.Data?.Items?.Count}");

                if (result?.Code == 0 && result.Data?.Items != null)
                {
                    CurrentCheckins.Clear();
                    foreach (var item in result.Data.Items)
                    {
                        CurrentCheckins.Add(item);
                    }
                    _hasLoadedCheckins = true;
                    OnPropertyChanged(nameof(HasNoCheckins));
                    OnPropertyChanged(nameof(CurrentCheckinMessage));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCurrentCheckinsAsync Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"LoadCurrentCheckinsAsync StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsLoadingCheckins = false;
            }
        }

        private void OnChangeHistoryRange(string range)
        {
            if (!string.IsNullOrWhiteSpace(range) && range != _historyRange)
            {
                HistoryRange = range;
            }
        }

        /// <summary>
        /// 将扁平的消费记录按消费日期(天)分组，供 CollectionView 分组显示。
        /// 组按日期倒序，组内按时间倒序。
        /// </summary>
        private void RebuildGroups()
        {
            var groups = ConsumptionHistory
                .GroupBy(i => i.ConsumptionTime.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new ConsumptionDateGroup(g.Key, g.OrderByDescending(i => i.ConsumptionTime)))
                .ToList();

            GroupedConsumptionHistory.Clear();
            foreach (var group in groups)
            {
                GroupedConsumptionHistory.Add(group);
            }
        }

        private async Task LoadConsumptionHistoryAsync()
        {
            try
            {
                IsLoadingHistory = true;
                _historyPage = 1;
                HasMoreHistory = true;

                var isLoggedIn = await _authService.HasValidTokenAsync();
                System.Diagnostics.Debug.WriteLine($"LoadConsumptionHistoryAsync: isLoggedIn={isLoggedIn}");
                if (!isLoggedIn)
                {
                    ConsumptionHistory.Clear();
                    return;
                }

                var httpService = MauiProgram.Services.GetService<IHttpService>();
                var url = $"MobileBooking/GetProductConsumptionHistory?Page={_historyPage}&PageSize={_historyPageSize}&Range={_historyRange}";
                System.Diagnostics.Debug.WriteLine($"LoadConsumptionHistoryAsync: requesting {url}");
                var response = await httpService.RequestAsync(url);

                System.Diagnostics.Debug.WriteLine($"LoadConsumptionHistoryAsync: StatusCode={response?.StatusCode}, MessageLength={response?.Message?.Length}");

                if (string.IsNullOrWhiteSpace(response?.Message))
                {
                    System.Diagnostics.Debug.WriteLine("LoadConsumptionHistoryAsync: empty response");
                    return;
                }

                var result = HttpHelper.JsonToModel<ApiResponse<PagedData<ConsumptionRecordDto>>>(response.Message);
                System.Diagnostics.Debug.WriteLine($"LoadConsumptionHistoryAsync: Code={result?.Code}, Items={result?.Data?.Items?.Count}");

                if (result?.Code == 0 && result.Data?.Items != null)
                {
                    foreach (var item in result.Data.Items)
                    {
                        System.Diagnostics.Debug.WriteLine($"HistoryItem: Product={item.ProductName}, Status={item.SettlementStatus}, Time={item.ConsumptionTime}");
                    }

                    var sorted = result.Data.Items
                        .OrderBy(i => i.SettlementStatus == "UnSettle" ? 0 : 1)
                        .ThenByDescending(i => i.ConsumptionTime)
                        .ToList();

                    ConsumptionHistory.Clear();
                    foreach (var item in sorted)
                    {
                        ConsumptionHistory.Add(item);
                    }
                    RebuildGroups();
                    _hasLoadedHistory = true;
                    HasMoreHistory = ConsumptionHistory.Count < result.Data.TotalCount;
                    OnPropertyChanged(nameof(HasNoConsumptionHistory));
                    System.Diagnostics.Debug.WriteLine($"LoadConsumptionHistoryAsync: loaded {ConsumptionHistory.Count} items");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadConsumptionHistoryAsync Exception: {ex}");
            }
            finally
            {
                IsLoadingHistory = false;
            }
        }

        private async Task LoadMoreHistoryAsync()
        {
            if (IsLoadingMoreHistory || !HasMoreHistory)
                return;

            try
            {
                IsLoadingMoreHistory = true;
                _historyPage++;

                var isLoggedIn = await _authService.HasValidTokenAsync();
                if (!isLoggedIn)
                {
                    return;
                }

                var httpService = MauiProgram.Services.GetService<IHttpService>();
                var response = await httpService.RequestAsync($"MobileBooking/GetProductConsumptionHistory?Page={_historyPage}&PageSize={_historyPageSize}&Range={_historyRange}");

                if (string.IsNullOrWhiteSpace(response?.Message))
                    return;

                var result = HttpHelper.JsonToModel<ApiResponse<PagedData<ConsumptionRecordDto>>>(response.Message);

                if (result?.Code == 0 && result.Data?.Items != null)
                {
                    foreach (var item in result.Data.Items)
                    {
                        ConsumptionHistory.Add(item);
                    }
                    RebuildGroups();
                    HasMoreHistory = ConsumptionHistory.Count < result.Data.TotalCount;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadMoreHistoryAsync Exception: {ex.Message}");
                _historyPage--;
            }
            finally
            {
                IsLoadingMoreHistory = false;
            }
        }

        private async Task SubmitReservationAsync()
        {
            try
            {
                if (!await _authService.HasValidTokenAsync())
                {
                    StatusMessage = "请先登录后再进行预约。";
                    StatusMessageColor = Colors.OrangeRed;
                    await _navigationService.NavigateToAsync($"//{nameof(LoginPage)}");
                    return;
                }

                var validationMessage = ValidateForm();
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    StatusMessage = validationMessage;
                    StatusMessageColor = Colors.OrangeRed;
                    return;
                }

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
                System.Diagnostics.Debug.WriteLine($"SubmitReservationAsync Exception: {ex}");
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

    public class CurrentCheckinDto : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string RoomName { get; set; }
        public string RoomNumber { get; set; }
        public string RoomArea { get; set; }
        public int? RoomFloor { get; set; }
        public DateTime CheckInTime { get; set; }
        public decimal DailyRate { get; set; }
        public string CustomerLevelName { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountedDailyRate { get; set; }
        public decimal ProductConsumption { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        /// <summary>
        /// 本次入住唯一标识 (Stay ID)，由服务端在办理入住/换房时生成；提交评价时回传用于校验
        /// </summary>
        public string StayId { get; set; } = string.Empty;

        public string DiscountDisplay => Discount > 0 && Discount < 100
            ? $"{Discount / 10:F0}折"
            : "无折扣";

        public bool HasProductConsumption => ProductConsumption > 0;

        public string DiscountSummary => Discount > 0 && Discount < 100
            ? $"{CustomerLevelName} {DiscountDisplay} | 折扣房费 ¥{DiscountedDailyRate:F0}/天"
            : $"房费 ¥{DailyRate:F0}/天";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ConsumptionRecordDto
    {
        public string RoomNumber { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public DateTime ConsumptionTime { get; set; }
        public string SettlementStatus { get; set; }
        public string StatusDisplay => SettlementStatus == "UnSettle" ? "未结算" : "已结算";
        public string StatusColor => SettlementStatus == "UnSettle" ? "#F59E0B" : "#10B981";
        public string Summary => $"房号 {RoomNumber} × {Quantity}  ¥{UnitPrice:F0}/件";
    }

    /// <summary>
    /// 消费记录按「消费日期(天)」分组的组容器，供 CollectionView 分组显示。
    /// 组本身是可枚举集合（承载当日明细），并额外暴露日期标题与小计。
    /// </summary>
    public class ConsumptionDateGroup : ObservableCollection<ConsumptionRecordDto>
    {
        public ConsumptionDateGroup(DateTime date, IEnumerable<ConsumptionRecordDto> items) : base(items)
        {
            Date = date;
        }

        public DateTime Date { get; }

        public string DateLabel
        {
            get
            {
                var today = DateTime.Today;
                if (Date.Date == today)
                    return $"今天 · {Date:yyyy年M月d日}";
                if (Date.Date == today.AddDays(-1))
                    return $"昨天 · {Date:yyyy年M月d日}";
                return $"{Date:yyyy年M月d日}";
            }
        }

        public decimal DayTotal => this.Sum(i => i.Amount);
    }
}
