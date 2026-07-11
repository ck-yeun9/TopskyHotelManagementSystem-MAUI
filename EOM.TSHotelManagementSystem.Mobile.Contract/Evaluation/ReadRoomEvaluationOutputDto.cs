using System;

namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    /// <summary>
    /// 房间体验评价输出
    /// </summary>
    public class ReadRoomEvaluationOutputDto
    {
        /// <summary>
        /// 评价ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 评价编号
        /// </summary>
        public string EvaluationNumber { get; set; } = string.Empty;

        /// <summary>
        /// 房间ID
        /// </summary>
        public int? RoomId { get; set; }

        /// <summary>
        /// 房号
        /// </summary>
        public string RoomNumber { get; set; } = string.Empty;

        /// <summary>
        /// 房型ID
        /// </summary>
        public int RoomTypeId { get; set; }

        /// <summary>
        /// 客户编号
        /// </summary>
        public string CustomerNumber { get; set; } = string.Empty;

        /// <summary>
        /// 客户姓名/昵称（匿名或未匹配时为空）
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// 是否匿名
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// 本次入住唯一标识 (Stay ID)
        /// </summary>
        public string StayId { get; set; } = string.Empty;

        /// <summary>
        /// 列表展示用用户名（由 ViewModel 加载后填充：匿名→"匿名用户"，否则优先姓名/编号）
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 卫生评分 (1-5)
        /// </summary>
        public int CleanlinessScore { get; set; }

        /// <summary>
        /// 服务评分 (1-5)
        /// </summary>
        public int ServiceScore { get; set; }

        /// <summary>
        /// 舒适度评分 (1-5)
        /// </summary>
        public int ComfortScore { get; set; }

        /// <summary>
        /// 设施与位置评分 (1-5)
        /// </summary>
        public int LocationScore { get; set; }

        /// <summary>
        /// 综合评分 (1-5)
        /// </summary>
        public int OverallScore { get; set; }

        /// <summary>
        /// 评价内容
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// 评价图片链接 (逗号分隔)
        /// </summary>
        public string PhotoUrls { get; set; } = string.Empty;

        /// <summary>
        /// 评价时间
        /// </summary>
        public DateTime? DataInsDate { get; set; }
    }
}
