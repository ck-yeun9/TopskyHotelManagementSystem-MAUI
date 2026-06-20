using EOM.TSHotelManagementSystem.Mobile.Contract;
using System.Diagnostics;
using System.Text.Json;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class NewsService : INewsService
    {
        private readonly IHttpService _httpService;

        public NewsService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<List<ReadNewsOutputDto>> GetNewsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _httpService.RequestAsync(
                    $"News/SelectNews?page={page}&pageSize={pageSize}");

                if (string.IsNullOrWhiteSpace(response?.Message))
                    return new List<ReadNewsOutputDto>();

                var result = HttpHelper.JsonToModel<ApiResponse<PagedData<ReadNewsOutputDto>>>(response.Message);

                if (result?.Code == 0 && result.Data?.Items != null)
                {
                    return result.Data.Items;
                }

                return new List<ReadNewsOutputDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetNewsAsync Exception: {ex.Message}");
                return new List<ReadNewsOutputDto>();
            }
        }

        public async Task<ReadNewsOutputDto> GetNewsDetailAsync(string newsId)
        {
            try
            {
                var response = await _httpService.RequestAsync(
                    $"News/News?newId={newsId}");

                if (string.IsNullOrWhiteSpace(response?.Message))
                    return null;

                var result = HttpHelper.JsonToModel<ApiResponse<ReadNewsOutputDto>>(response.Message);

                if (result?.Code == 0 && result.Data != null)
                {
                    return result.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetNewsDetailAsync Exception: {ex.Message}");
                return null;
            }
        }
    }
}
