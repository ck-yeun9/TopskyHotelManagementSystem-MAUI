namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class StaffLoginOutputDto : BaseDto
    {
        public string Account { get; set; }
        public string Name { get; set; }
        public int StaffRoleId { get; set; }
        public string StaffRoleName { get; set; }
        public int Status { get; set; } = 0;
    }
}
