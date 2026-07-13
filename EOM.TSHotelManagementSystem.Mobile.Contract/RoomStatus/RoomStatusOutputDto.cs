namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class RoomStatusOutputDto : BaseOutputDto
    {
        public int RoomId { get; set; }
        public string RoomNo { get; set; }
        public int FloorId { get; set; }
        public string FloorName { get; set; }
        public int RoomState { get; set; }
        public string RoomStateName { get; set; }
        public string RoomDesc { get; set; }
        public string RoomTypeName { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
