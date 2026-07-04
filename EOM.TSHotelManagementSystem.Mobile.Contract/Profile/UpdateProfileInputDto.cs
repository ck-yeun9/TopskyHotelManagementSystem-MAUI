namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class UpdateProfileInputDto
    {
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Gender { get; set; }
        public string Ethnicity { get; set; }
        public string EducationLevel { get; set; }
    }
}
