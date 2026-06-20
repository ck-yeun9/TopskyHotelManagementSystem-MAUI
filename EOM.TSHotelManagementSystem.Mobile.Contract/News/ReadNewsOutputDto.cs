namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class ReadNewsOutputDto
    {
        public string NewId { get; set; }
        public string NewsTitle { get; set; }
        public string NewsContent { get; set; }
        public string NewsType { get; set; }
        public string NewsTypeDescription { get; set; }
        public string NewsLink { get; set; }
        public DateTime NewsDate { get; set; }
        public string NewsStatus { get; set; }
        public string NewsStatusDescription { get; set; }
        public string NewsImage { get; set; }
    }
}
