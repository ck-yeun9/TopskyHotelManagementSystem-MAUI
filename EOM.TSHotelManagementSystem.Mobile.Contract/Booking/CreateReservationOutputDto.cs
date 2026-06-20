namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class CreateReservationOutputDto
    {
        public string ReservationNumber { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int GuestCount { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
