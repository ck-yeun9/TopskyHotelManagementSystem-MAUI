namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class ReadNewsInputDto : ListInputDto
    {
        public string? NewId { get; set; }
        public string? NewsTitle { get; set; }
        public string? NewsType { get; set; }
        public string? NewsStatus { get; set; }
    }
}
