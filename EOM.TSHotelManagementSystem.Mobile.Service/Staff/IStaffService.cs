using EOM.TSHotelManagementSystem.Mobile.Contract;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IStaffService
    {
        Task<bool> StaffLoginAsync(string username, string password);
        Task<bool> HasValidStaffTokenAsync();
        Task ClearStaffTokenAsync();

        Task<SingleOutputDto<RoomStatusOutputDto>> UpdateRoomStatusAsync(RoomStatusInputDto input);
        Task<ListOutputDto<RoomStatusOutputDto>> GetRoomStatusListAsync(int floorId);

        Task<SingleOutputDto<MeterReadingOutputDto>> AddMeterReadingAsync(MeterReadingInputDto input);
        Task<ListOutputDto<MeterReadingOutputDto>> GetMeterReadingListAsync(int floorId);

        Task<SingleOutputDto<MaterialOperationOutputDto>> ExecuteMaterialOperationAsync(MaterialOperationInputDto input);
        Task<ListOutputDto<MaterialOutputDto>> GetMaterialListAsync();
    }
}
