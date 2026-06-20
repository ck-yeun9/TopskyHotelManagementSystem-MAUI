namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class ReadReserOutputDto
    {
        public int? Id { get; set; }
        public int? RoomId { get; set; }
        public string ReservationId { get; set; }
        public string CustomerName { get; set; }
        public string ReservationPhoneNumber { get; set; }
        public string ReservationRoomNumber { get; set; }
        public string RoomArea { get; set; }
        public int? RoomFloor { get; set; }
        public string RoomLocator { get; set; }
        public string ReservationChannel { get; set; }
        public string ReservationChannelDescription { get; set; }
        public DateTime ReservationStartDate { get; set; }
        public DateTime ReservationEndDate { get; set; }
    }
}
