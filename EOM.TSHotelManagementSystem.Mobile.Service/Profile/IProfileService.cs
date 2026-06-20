using EOM.TSHotelManagementSystem.Mobile.Contract;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IProfileService
    {
        Task<UserProfileOutputDto> GetUserProfileAsync();
    }
}
