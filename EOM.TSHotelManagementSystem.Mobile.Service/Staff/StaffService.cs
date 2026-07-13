using EOM.TSHotelManagementSystem.Mobile.Common.Utility;
using EOM.TSHotelManagementSystem.Mobile.Contract;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class StaffService : IStaffService
    {
        private readonly IHttpService _httpService;
        private const string StaffTokenKey = "StaffAccessToken";

        public StaffService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<bool> StaffLoginAsync(string username, string password)
        {
            try
            {
                var response = await _httpService.RequestAsync(
                    "StaffAccount/Login",
                    json: JsonSerializer.Serialize(new StaffLoginInputDto
                    {
                        Account = username,
                        Password = password
                    })
                );

                var sourceResponse = HttpHelper.JsonToModel<SingleOutputDto<StaffLoginOutputDto>>(response.Message!);

                if (sourceResponse.StatusCode == StatusCodeConstants.Success)
                {
                    await SaveStaffTokenAsync(sourceResponse.Source.UserToken);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StaffLoginAsync Exception: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> HasValidStaffTokenAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync(StaffTokenKey);
                return !string.IsNullOrEmpty(token);
            }
            catch
            {
                var token = Preferences.Get(StaffTokenKey, string.Empty);
                return !string.IsNullOrEmpty(token);
            }
        }

        public async Task ClearStaffTokenAsync()
        {
            try
            {
                SecureStorage.Remove(StaffTokenKey);
            }
            finally
            {
                Preferences.Remove(StaffTokenKey);
            }
            await Task.CompletedTask;
        }

        public async Task<SingleOutputDto<RoomStatusOutputDto>> UpdateRoomStatusAsync(RoomStatusInputDto input)
        {
            try
            {
                var response = await _httpService.RequestAsync(
                    "RoomStatus/UpdateRoomStatus",
                    json: HttpHelper.ModelToJson(input)
                );

                return HttpHelper.JsonToModel<SingleOutputDto<RoomStatusOutputDto>>(response.Message!);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateRoomStatusAsync Exception: {ex.Message}");
                return new SingleOutputDto<RoomStatusOutputDto>
                {
                    StatusCode = StatusCodeConstants.InternalServerError,
                    Message = $"更新房间状态失败: {ex.Message}"
                };
            }
        }

        public async Task<ListOutputDto<RoomStatusOutputDto>> GetRoomStatusListAsync(int floorId)
        {
            try
            {
                var response = await _httpService.RequestAsync(
                    $"RoomStatus/GetRoomStatusList?floorId={floorId}"
                );

                return HttpHelper.JsonToModel<ListOutputDto<RoomStatusOutputDto>>(response.Message!);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetRoomStatusListAsync Exception: {ex.Message}");
                return new ListOutputDto<RoomStatusOutputDto>
                {
                    StatusCode = StatusCodeConstants.InternalServerError,
                    Message = $"获取房间状态列表失败: {ex.Message}"
                };
            }
        }

        public async Task<SingleOutputDto<MeterReadingOutputDto>> AddMeterReadingAsync(MeterReadingInputDto input)
        {
            try
            {
                var response = await _httpService.RequestAsync(
                    "MeterReading/AddMeterReading",
                    json: HttpHelper.ModelToJson(input)
                );

                return HttpHelper.JsonToModel<SingleOutputDto<MeterReadingOutputDto>>(response.Message!);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddMeterReadingAsync Exception: {ex.Message}");
                return new SingleOutputDto<MeterReadingOutputDto>
                {
                    StatusCode = StatusCodeConstants.InternalServerError,
                    Message = $"提交抄表记录失败: {ex.Message}"
                };
            }
        }

        public async Task<ListOutputDto<MeterReadingOutputDto>> GetMeterReadingListAsync(int floorId)
        {
            try
            {
                var response = await _httpService.RequestAsync(
                    $"MeterReading/GetMeterReadingList?floorId={floorId}"
                );

                return HttpHelper.JsonToModel<ListOutputDto<MeterReadingOutputDto>>(response.Message!);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetMeterReadingListAsync Exception: {ex.Message}");
                return new ListOutputDto<MeterReadingOutputDto>
                {
                    StatusCode = StatusCodeConstants.InternalServerError,
                    Message = $"获取抄表记录失败: {ex.Message}"
                };
            }
        }

        public async Task<SingleOutputDto<MaterialOperationOutputDto>> ExecuteMaterialOperationAsync(MaterialOperationInputDto input)
        {
            try
            {
                var response = await _httpService.RequestAsync(
                    "Material/ExecuteOperation",
                    json: HttpHelper.ModelToJson(input)
                );

                return HttpHelper.JsonToModel<SingleOutputDto<MaterialOperationOutputDto>>(response.Message!);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteMaterialOperationAsync Exception: {ex.Message}");
                return new SingleOutputDto<MaterialOperationOutputDto>
                {
                    StatusCode = StatusCodeConstants.InternalServerError,
                    Message = $"执行物资操作失败: {ex.Message}"
                };
            }
        }

        public async Task<ListOutputDto<MaterialOutputDto>> GetMaterialListAsync()
        {
            try
            {
                var response = await _httpService.RequestAsync("Material/GetMaterialList");

                return HttpHelper.JsonToModel<ListOutputDto<MaterialOutputDto>>(response.Message!);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetMaterialListAsync Exception: {ex.Message}");
                return new ListOutputDto<MaterialOutputDto>
                {
                    StatusCode = StatusCodeConstants.InternalServerError,
                    Message = $"获取物资列表失败: {ex.Message}"
                };
            }
        }

        private async Task SaveStaffTokenAsync(string token)
        {
            try
            {
                await SecureStorage.SetAsync(StaffTokenKey, token);
            }
            catch
            {
                Preferences.Set(StaffTokenKey, token);
            }
        }
    }
}
