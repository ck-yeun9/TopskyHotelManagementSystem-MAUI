using EOM.TSHotelManagementSystem.Mobile.Contract;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IProfileService
    {
        Task<UserProfileOutputDto> GetUserProfileAsync();
        Task<bool> UpdateProfileAsync(UpdateProfileInputDto input);
        Task<List<CustoTypeOutputDto>> GetCustomerTypesAsync();
        Task<List<NationOutputDto>> GetNationsAsync();
        Task<List<EducationOutputDto>> GetEducationsAsync();
    }
}
