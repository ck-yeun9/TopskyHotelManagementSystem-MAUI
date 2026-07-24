using EOM.TSHotelManagementSystem.Mobile.Contract;
using System.Diagnostics;
using System.Linq;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class RealBookingService : IBookingService
    {
        private readonly IHttpService _httpService;
        private readonly IAuthService _authService;

        public RealBookingService(IHttpService httpService, IAuthService authService)
        {
            _httpService = httpService;
            _authService = authService;
        }

        public async Task<IReadOnlyList<AvailableRoomDto>> GetAvailableRoomsAsync(
            DateTime checkInDate,
            DateTime checkOutDate,
            int guestCount)
        {
            try
            {
                var roomTypeResponse = await _httpService.RequestAsync("MobileBooking/GetRoomTypes");
                var availableRoomResponse = await _httpService.RequestAsync("MobileBooking/GetAvailableRooms");

                Debug.WriteLine($"[GetAvailableRooms] RoomType Response: {roomTypeResponse?.Message}");
                Debug.WriteLine($"[GetAvailableRooms] AvailableRoom Response: {availableRoomResponse?.Message}");

                if (string.IsNullOrWhiteSpace(roomTypeResponse?.Message))
                    return new List<AvailableRoomDto>();

                var roomTypeResult = HttpHelper.JsonToModel<ApiResponse<PagedData<ReadRoomTypeOutputDto>>>(roomTypeResponse.Message);

                if (roomTypeResult?.Code != 0 || roomTypeResult.Data?.Items == null)
                {
                    Debug.WriteLine($"[GetAvailableRooms] Failed: {roomTypeResult?.Message}");
                    return new List<AvailableRoomDto>();
                }

                var roomCountsByType = new Dictionary<int, int>();
                if (!string.IsNullOrWhiteSpace(availableRoomResponse?.Message))
                {
                    var availableRoomResult = HttpHelper.JsonToModel<ApiResponse<PagedData<ReadRoomOutputDto>>>(availableRoomResponse.Message);
                    if (availableRoomResult?.Code == 0 && availableRoomResult.Data?.Items != null)
                    {
                        roomCountsByType = availableRoomResult.Data.Items
                            .Where(r => r.RoomStateId == 1)
                            .GroupBy(r => r.RoomTypeId)
                            .ToDictionary(g => g.Key, g => g.Count());
                    }
                }

                return roomTypeResult.Data.Items.Select(rt => new AvailableRoomDto
                {
                    RoomTypeId = rt.RoomTypeId.ToString(),
                    RoomName = rt.RoomTypeName,
                    Description = $"{rt.RoomTypeName} - 每日租金 ¥{rt.RoomRent:F0}",
                    BedType = rt.RoomTypeName,
                    MaxGuests = guestCount,
                    IncludesBreakfast = false,
                    PricePerNight = rt.RoomRent,
                    RemainingRooms = roomCountsByType.TryGetValue(rt.RoomTypeId, out var count) ? count : 0
                }).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetAvailableRooms] Exception: {ex.Message}");
                return new List<AvailableRoomDto>();
            }
        }

        public async Task<CreateReservationOutputDto> CreateReservationAsync(CreateReservationInputDto input)
        {
            try
            {
                // 从可用房间列表中查找所选房型下的一个具体房间
                var availableRoomResponse = await _httpService.RequestAsync("MobileBooking/GetAvailableRooms");
                if (string.IsNullOrWhiteSpace(availableRoomResponse?.Message))
                    throw new InvalidOperationException("无法获取可用房间列表");

                var availableRoomResult = HttpHelper.JsonToModel<ApiResponse<PagedData<ReadRoomOutputDto>>>(availableRoomResponse.Message);
                if (availableRoomResult?.Code != 0 || availableRoomResult.Data?.Items == null)
                    throw new InvalidOperationException("获取可用房间列表失败");

                var targetRoomTypeId = int.TryParse(input.RoomTypeId, out var parsedTypeId) ? parsedTypeId : 0;
                var targetRoom = availableRoomResult.Data.Items
                    .FirstOrDefault(r => r.RoomTypeId == targetRoomTypeId && r.RoomStateId == 1);

                if (targetRoom == null)
                    throw new InvalidOperationException("所选房型已无可用房间，请重新搜索");

                var reservationId = $"RSE-{DateTime.Now:yyyyMMddHHmmss}";

                // 获取客户编号存入 Remarks，以便 GetMyReservations 按编号查询
                var customerNumber = await _authService.GetCustomerNumberAsync();
                if (string.IsNullOrEmpty(customerNumber))
                    throw new InvalidOperationException("无法获取客户信息，请重新登录");

                var remarks = string.IsNullOrWhiteSpace(input.SpecialRequest)
                    ? customerNumber
                    : $"{customerNumber}|{input.SpecialRequest}";

                var reserInput = new CreateReserInputDto
                {
                    ReservationId = reservationId,
                    CustomerName = input.ContactName,
                    ReservationPhoneNumber = input.PhoneNumber,
                    RoomId = targetRoom.Id,
                    ReservationRoomNumber = targetRoom.RoomNumber,
                    ReservationChannel = "App",
                    ReservationStartDate = input.CheckInDate,
                    ReservationEndDate = input.CheckOutDate,
                    ReservationStatus = 0,
                    Remarks = remarks
                };

                var json = HttpHelper.ModelToJson(reserInput);
                var response = await _httpService.RequestAsync("MobileBooking/CreateReservation", json: json);

                if (string.IsNullOrWhiteSpace(response?.Message))
                    throw new InvalidOperationException("服务器无响应");

                var result = HttpHelper.JsonToModel<ApiResponse>(response.Message);

                if (result?.Code == 0)
                {
                    var nights = Math.Max(1, (input.CheckOutDate.Date - input.CheckInDate.Date).Days);

                    var roomTypeResponse = await _httpService.RequestAsync("MobileBooking/GetRoomTypes");
                    if (string.IsNullOrWhiteSpace(roomTypeResponse?.Message))
                        throw new InvalidOperationException("无法获取房型信息");

                    var roomTypeResult = HttpHelper.JsonToModel<ApiResponse<PagedData<ReadRoomTypeOutputDto>>>(roomTypeResponse.Message);
                    var roomType = roomTypeResult?.Data?.Items?.FirstOrDefault(rt => rt.RoomTypeId == targetRoomTypeId);
                    var pricePerNight = roomType?.RoomRent ?? 0;

                    return new CreateReservationOutputDto
                    {
                        ReservationNumber = reservationId,
                        RoomName = input.RoomName,
                        CheckInDate = input.CheckInDate,
                        CheckOutDate = input.CheckOutDate,
                        GuestCount = input.GuestCount,
                        TotalAmount = pricePerNight * nights,
                        Status = "已确认"
                    };
                }

                throw new InvalidOperationException(result?.Message ?? "预约失败");
            }
            catch (Exception ex)
            {
                WriteLog($"CreateReservationAsync Exception: {ex.Message}");
                throw;
            }
        }

        private static void WriteLog(string message)
        {
            try
            {
#if ANDROID
                var dir = Android.App.Application.Context.GetExternalFilesDir(null)!.AbsolutePath;
#else
                var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
#endif
                var logPath = Path.Combine(dir, "crash.log");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
            }
            catch
            {
                // 备用路径
                try
                {
                    var fallback = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                    var logPath = Path.Combine(fallback, "crash.log");
                    File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
                }
                catch { }
            }
        }
    }
}
