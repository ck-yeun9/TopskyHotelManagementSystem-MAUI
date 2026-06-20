namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class CreateReserInputDto
    {
        public string ReservationId { get; set; }
        public string CustomerName { get; set; }
        public string ReservationPhoneNumber { get; set; }
        public int? RoomId { get; set; }
        public string ReservationRoomNumber { get; set; }
        public string ReservationChannel { get; set; }
        public DateTime ReservationStartDate { get; set; }
        public DateTime ReservationEndDate { get; set; }
        public int ReservationStatus { get; set; } = 0;
        public string Remarks { get; set; }
    }
}
