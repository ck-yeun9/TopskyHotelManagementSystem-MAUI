using EOM.TSHotelManagementSystem.Mobile.Contract;
using System.Diagnostics;
using System.Text.Json;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class ProfileService : IProfileService
    {
        private readonly IHttpService _httpService;

        public ProfileService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<UserProfileOutputDto> GetUserProfileAsync()
        {
            try
            {
                var response = await _httpService.RequestAsync("Profile/GetUserProfile");

                Debug.WriteLine($"[ProfileService] Response StatusCode: {response?.StatusCode}");
                Debug.WriteLine($"[ProfileService] Response Message: {response?.Message}");

                if (string.IsNullOrWhiteSpace(response?.Message))
                {
                    Debug.WriteLine("[ProfileService] 服务器无响应");
                    return null;
                }

                // 先尝试解析为 JsonDocument 查看原始结构
                try
                {
                    using var doc = JsonDocument.Parse(response.Message);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("code", out var codeProp) || root.TryGetProperty("Code", out codeProp))
                    {
                        Debug.WriteLine($"[ProfileService] JSON Code: {codeProp.GetInt32()}");
                    }
                    if (root.TryGetProperty("data", out var dataProp) || root.TryGetProperty("Data", out dataProp))
                    {
                        Debug.WriteLine($"[ProfileService] JSON Data: {dataProp.GetRawText()}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ProfileService] JSON解析异常: {ex.Message}");
                }

                var result = HttpHelper.JsonToModel<ApiResponse<UserProfileOutputDto>>(response.Message);

                Debug.WriteLine($"[ProfileService] Result Code: {result?.Code}");
                Debug.WriteLine($"[ProfileService] Result Data is null: {result?.Data == null}");

                if (result?.Code == 0 && result.Data != null)
                {
                    Debug.WriteLine($"[ProfileService] Account: {result.Data.Account}");
                    Debug.WriteLine($"[ProfileService] DisplayName: {result.Data.DisplayName}");
                    return result.Data;
                }

                Debug.WriteLine($"[ProfileService] 请求失败: {result?.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileService] Exception: {ex.Message}");
                Debug.WriteLine($"[ProfileService] StackTrace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileInputDto input)
        {
            try
            {
                var json = HttpHelper.ModelToJson(input);
                var response = await _httpService.RequestAsync("Profile/UpdateProfile", json: json);

                if (string.IsNullOrWhiteSpace(response?.Message))
                    return false;

                var result = HttpHelper.JsonToModel<ApiResponse>(response.Message);
                return result?.Code == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileService] UpdateProfileAsync Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CustoTypeOutputDto>> GetCustomerTypesAsync()
        {
            try
            {
                var response = await _httpService.RequestAsync("MobileBooking/GetCustomerTypes");
                if (string.IsNullOrWhiteSpace(response?.Message)) return new List<CustoTypeOutputDto>();

                var result = HttpHelper.JsonToModel<ApiResponse<PagedData<CustoTypeOutputDto>>>(response.Message);
                return result?.Code == 0 ? result.Data?.Items ?? new List<CustoTypeOutputDto>() : new List<CustoTypeOutputDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileService] GetCustomerTypesAsync Exception: {ex.Message}");
                return new List<CustoTypeOutputDto>();
            }
        }

        public async Task<List<NationOutputDto>> GetNationsAsync()
        {
            try
            {
                var response = await _httpService.RequestAsync("MobileBooking/GetNations");
                if (string.IsNullOrWhiteSpace(response?.Message)) return new List<NationOutputDto>();

                var result = HttpHelper.JsonToModel<ApiResponse<PagedData<NationOutputDto>>>(response.Message);
                return result?.Code == 0 ? result.Data?.Items ?? new List<NationOutputDto>() : new List<NationOutputDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileService] GetNationsAsync Exception: {ex.Message}");
                return new List<NationOutputDto>();
            }
        }

        public async Task<List<EducationOutputDto>> GetEducationsAsync()
        {
            try
            {
                var response = await _httpService.RequestAsync("MobileBooking/GetEducations");
                if (string.IsNullOrWhiteSpace(response?.Message)) return new List<EducationOutputDto>();

                var result = HttpHelper.JsonToModel<ApiResponse<PagedData<EducationOutputDto>>>(response.Message);
                return result?.Code == 0 ? result.Data?.Items ?? new List<EducationOutputDto>() : new List<EducationOutputDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileService] GetEducationsAsync Exception: {ex.Message}");
                return new List<EducationOutputDto>();
            }
        }
    }
}
