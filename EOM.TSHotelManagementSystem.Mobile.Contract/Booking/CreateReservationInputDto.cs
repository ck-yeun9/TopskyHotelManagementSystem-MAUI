namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class CreateReservationInputDto
    {
        public string RoomTypeId { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int GuestCount { get; set; }

        public string ContactName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string SpecialRequest { get; set; } = string.Empty;
    }
}
