using EOM.TSHotelManagementSystem.Mobile.Contract;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IBookingService
    {
        Task<IReadOnlyList<AvailableRoomDto>> GetAvailableRoomsAsync(
            DateTime checkInDate,
            DateTime checkOutDate,
            int guestCount);

        Task<CreateReservationOutputDto> CreateReservationAsync(CreateReservationInputDto input);
    }
}
