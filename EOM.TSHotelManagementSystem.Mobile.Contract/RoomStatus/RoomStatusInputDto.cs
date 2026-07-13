namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class RoomStatusInputDto : BaseInputDto
    {
        public int RoomId { get; set; }
        public int FloorId { get; set; }
        public int RoomState { get; set; }
        public string RoomDesc { get; set; }
    }
}
