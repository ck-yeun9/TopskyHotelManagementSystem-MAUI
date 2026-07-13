namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class MaterialOutputDto : BaseOutputDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string MaterialTypeName { get; set; }
        public int StockQuantity { get; set; }
        public string UnitName { get; set; }
        public DateTime? LastOperationDate { get; set; }
    }
}
