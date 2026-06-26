using EOM.TSHotelManagementSystem.Mobile.Contract;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IReservationService
    {
        Task<List<ReadReserOutputDto>> GetMyReservationsAsync();
    }
}
