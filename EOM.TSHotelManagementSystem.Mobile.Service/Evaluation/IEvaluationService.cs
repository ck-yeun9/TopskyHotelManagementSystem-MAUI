using EOM.TSHotelManagementSystem.Mobile.Contract;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    /// <summary>
    /// 房间体验评价服务
    /// </summary>
    public interface IEvaluationService
    {
        /// <summary>
        /// 提交评价
        /// </summary>
        /// <returns>Success 是否成功；ErrorMessage 为服务端返回的真实错误消息（成功时为空）</returns>
        Task<(bool Success, string ErrorMessage)> SubmitEvaluationAsync(CreateEvaluationInputDto input);

        /// <summary>
        /// 获取我的评价
        /// </summary>
        Task<List<ReadRoomEvaluationOutputDto>> GetMyEvaluationsAsync(int page = 1, int pageSize = 15);

        /// <summary>
        /// 获取房间评价统计
        /// </summary>
        Task<RoomEvaluationStatisticsOutputDto> GetStatisticsAsync(string roomNumber);

        /// <summary>
        /// 分页获取某房间的评价列表（公开，懒加载用）
        /// </summary>
        Task<PagedData<ReadRoomEvaluationOutputDto>> GetRoomEvaluationsAsync(string roomNumber, int page = 1, int pageSize = 10);
    }
}
