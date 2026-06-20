using EOM.TSHotelManagementSystem.Mobile.Contract;
using System.Diagnostics;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class ReservationService : IReservationService
    {
        private readonly IHttpService _httpService;

        public ReservationService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<List<ReadReserOutputDto>> GetMyReservationsAsync(string customerName)
        {
            try
            {
                var response = await _httpService.RequestAsync(
                    $"MobileBooking/GetReservations?customerName={Uri.EscapeDataString(customerName)}");

                if (string.IsNullOrWhiteSpace(response?.Message))
                    return new List<ReadReserOutputDto>();

                var result = HttpHelper.JsonToModel<ApiResponse<PagedData<ReadReserOutputDto>>>(response.Message);

                if (result?.Code == 0 && result.Data?.Items != null)
                {
                    return result.Data.Items;
                }

                return new List<ReadReserOutputDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetMyReservationsAsync Exception: {ex.Message}");
                return new List<ReadReserOutputDto>();
            }
        }
    }
}
