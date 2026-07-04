namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class UserProfileOutputDto
    {
        public string LoginType { get; set; }
        public string UserNumber { get; set; }
        public string Account { get; set; }
        public string DisplayName { get; set; }
        public string PhotoUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Gender { get; set; }
        public int? CustomerType { get; set; }
        public string Ethnicity { get; set; }
        public string EducationLevel { get; set; }
    }
}
