namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class MeterReadingOutputDto : BaseOutputDto
    {
        public int ReadingId { get; set; }
        public int RoomId { get; set; }
        public string RoomNo { get; set; }
        public int FloorId { get; set; }
        public string FloorName { get; set; }
        public decimal WaterReading { get; set; }
        public decimal ElectricReading { get; set; }
        public string ReadingNote { get; set; }
        public string OperatorName { get; set; }
        public DateTime? ReadingDate { get; set; }
    }
}
