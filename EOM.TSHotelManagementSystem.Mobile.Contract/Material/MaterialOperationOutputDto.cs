namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class MaterialOperationOutputDto : BaseOutputDto
    {
        public int OperationId { get; set; }
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public int OperationType { get; set; }
        public string OperationTypeName { get; set; }
        public int Quantity { get; set; }
        public string OperationNote { get; set; }
        public string OperatorName { get; set; }
        public DateTime? OperationDate { get; set; }
    }
}
