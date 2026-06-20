using EOM.TSHotelManagementSystem.Mobile.Contract;
using System.Diagnostics;
using System.Linq;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class RealBookingService : IBookingService
    {
        private readonly IHttpService _httpService;

        public RealBookingService(IHttpService httpService)
        {
            _httpService = httpService;
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
                var reservationId = $"TS{DateTime.Now:yyyyMMddHHmmss}";
                var roomId = int.TryParse(input.RoomTypeId, out var id) ? id : (int?)null;

                var reserInput = new CreateReserInputDto
                {
                    ReservationId = reservationId,
                    CustomerName = input.ContactName,
                    ReservationPhoneNumber = input.PhoneNumber,
                    RoomId = roomId,
                    ReservationRoomNumber = input.RoomName,
                    ReservationChannel = "APP",
                    ReservationStartDate = input.CheckInDate,
                    ReservationEndDate = input.CheckOutDate,
                    ReservationStatus = 0,
                    Remarks = input.SpecialRequest
                };

                var json = HttpHelper.ModelToJson(reserInput);
                var response = await _httpService.RequestAsync("MobileBooking/CreateReservation", json: json);

                if (string.IsNullOrWhiteSpace(response?.Message))
                    throw new InvalidOperationException("服务器无响应");

                var result = HttpHelper.JsonToModel<ApiResponse>(response.Message);

                if (result?.Code == 0)
                {
                    var nights = Math.Max(1, (input.CheckOutDate.Date - input.CheckInDate.Date).Days);

                    var roomResponse = await _httpService.RequestAsync("MobileBooking/GetRoomTypes");
                    if (string.IsNullOrWhiteSpace(roomResponse?.Message))
                        throw new InvalidOperationException("无法获取房间信息");

                    var roomResult = HttpHelper.JsonToModel<ApiResponse<PagedData<ReadRoomTypeOutputDto>>>(roomResponse.Message);
                    var roomType = roomResult?.Data?.Items?.FirstOrDefault(rt => rt.RoomTypeId.ToString() == input.RoomTypeId);
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
                Debug.WriteLine($"CreateReservationAsync Exception: {ex.Message}");
                throw;
            }
        }
    }
}
