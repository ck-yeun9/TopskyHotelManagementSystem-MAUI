using System.Collections.Generic;

namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    /// <summary>
    /// 房间体验评价统计
    /// </summary>
    public class RoomEvaluationStatisticsOutputDto
    {
        /// <summary>
        /// 评价总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 卫生平均分
        /// </summary>
        public decimal AvgCleanliness { get; set; }

        /// <summary>
        /// 服务平均分
        /// </summary>
        public decimal AvgService { get; set; }

        /// <summary>
        /// 舒适度平均分
        /// </summary>
        public decimal AvgComfort { get; set; }

        /// <summary>
        /// 设施与位置平均分
        /// </summary>
        public decimal AvgLocation { get; set; }

        /// <summary>
        /// 综合平均分
        /// </summary>
        public decimal AvgOverall { get; set; }

        /// <summary>
        /// 综合评分分布（键为 1-5 星，值为数量）
        /// </summary>
        public Dictionary<int, int> Distribution { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// 最近点评列表
        /// </summary>
        public List<ReadRoomEvaluationOutputDto> RecentComments { get; set; } = new List<ReadRoomEvaluationOutputDto>();
    }
}
