using EOM.TSHotelManagementSystem.Mobile.Contract;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    /// <summary>
    /// 房间体验评价服务实现
    /// </summary>
    public class EvaluationService : IEvaluationService
    {
        private readonly IHttpService _httpService;
        private readonly IAuthService _authService;

        public EvaluationService(IHttpService httpService, IAuthService authService)
        {
            _httpService = httpService;
            _authService = authService;
        }

        public async Task<(bool Success, string ErrorMessage)> SubmitEvaluationAsync(CreateEvaluationInputDto input)
        {
            if (!await _authService.HasValidTokenAsync())
                return (false, "登录已失效，请重新登录后再评价。");

            var json = HttpHelper.ModelToJson(input);
            var response = await _httpService.RequestAsync("RoomEvaluation/CreateEvaluation", json: json);
            if (string.IsNullOrWhiteSpace(response?.Message))
                return (false, "服务无响应，请稍后重试。");

            var result = HttpHelper.JsonToModel<ApiResponse>(response.Message);
            if (result?.Code == 0)
                return (true, string.Empty);

            // 透传服务端真实错误消息（如“您当前未入住该房间，无法评价”“缺少入住标识”等）
            return (false, result?.Message ?? "评价提交失败，请稍后重试。");
        }

        public async Task<List<ReadRoomEvaluationOutputDto>> GetMyEvaluationsAsync(int page = 1, int pageSize = 15)
        {
            var url = $"RoomEvaluation/GetMyEvaluations?Page={page}&PageSize={pageSize}";
            var response = await _httpService.RequestAsync(url);
            if (string.IsNullOrWhiteSpace(response?.Message))
                return new List<ReadRoomEvaluationOutputDto>();

            var result = HttpHelper.JsonToModel<ApiResponse<PagedData<ReadRoomEvaluationOutputDto>>>(response.Message);
            return result?.Code == 0 && result.Data?.Items != null
                ? result.Data.Items
                : new List<ReadRoomEvaluationOutputDto>();
        }

        public async Task<RoomEvaluationStatisticsOutputDto> GetStatisticsAsync(string roomNumber)
        {
            var url = $"RoomEvaluation/GetStatistics?roomNumber={Uri.EscapeDataString(roomNumber)}";
            var response = await _httpService.RequestAsync(url);
            if (string.IsNullOrWhiteSpace(response?.Message))
                return new RoomEvaluationStatisticsOutputDto();

            var result = HttpHelper.JsonToModel<ApiResponse<RoomEvaluationStatisticsOutputDto>>(response.Message);
            return result?.Code == 0 && result.Data != null
                ? result.Data
                : new RoomEvaluationStatisticsOutputDto();
        }

        public async Task<PagedData<ReadRoomEvaluationOutputDto>> GetRoomEvaluationsAsync(string roomNumber, int page = 1, int pageSize = 10)
        {
            var url = $"RoomEvaluation/GetRoomEvaluations?roomNumber={Uri.EscapeDataString(roomNumber)}&page={page}&pageSize={pageSize}";
            var response = await _httpService.RequestAsync(url);
            if (string.IsNullOrWhiteSpace(response?.Message))
                return new PagedData<ReadRoomEvaluationOutputDto> { Items = new List<ReadRoomEvaluationOutputDto>() };

            var result = HttpHelper.JsonToModel<ApiResponse<PagedData<ReadRoomEvaluationOutputDto>>>(response.Message);
            return result?.Code == 0 && result.Data != null
                ? result.Data
                : new PagedData<ReadRoomEvaluationOutputDto> { Items = new List<ReadRoomEvaluationOutputDto>() };
        }
    }
}
