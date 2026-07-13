namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class MaterialOperationInputDto : BaseInputDto
    {
        public int MaterialId { get; set; }
        public int OperationType { get; set; }
        public int Quantity { get; set; }
        public string OperationNote { get; set; }
    }
}
