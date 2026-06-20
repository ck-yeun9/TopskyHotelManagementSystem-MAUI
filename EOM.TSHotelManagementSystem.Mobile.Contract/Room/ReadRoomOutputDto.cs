namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class ReadRoomOutputDto
    {
        public int? Id { get; set; }
        public string RoomNumber { get; set; }
        public string RoomArea { get; set; }
        public int? RoomFloor { get; set; }
        public string RoomLocator { get; set; }
        public string RoomName { get; set; }
        public int RoomTypeId { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime? LastCheckInTime { get; set; }
        public DateTime? LastCheckOutTime { get; set; }
        public int RoomStateId { get; set; }
        public string RoomState { get; set; }
        public decimal RoomRent { get; set; }
        public decimal RoomDeposit { get; set; }
        public string RoomLocation { get; set; }
    }
}
