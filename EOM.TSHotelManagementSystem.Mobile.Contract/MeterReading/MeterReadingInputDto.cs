namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class MeterReadingInputDto : BaseInputDto
    {
        public int RoomId { get; set; }
        public int FloorId { get; set; }
        public decimal WaterReading { get; set; }
        public decimal ElectricReading { get; set; }
        public string ReadingNote { get; set; }
    }
}
