namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class ReadRoomTypeOutputDto
    {
        public int? Id { get; set; }
        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; }
        public decimal RoomRent { get; set; }
        public decimal RoomDeposit { get; set; }
    }
}
