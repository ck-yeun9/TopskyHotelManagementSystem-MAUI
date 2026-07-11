namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    /// <summary>
    /// 提交房间体验评价输入
    /// </summary>
    public class CreateEvaluationInputDto
    {
        /// <summary>
        /// 房号 (Room Number)
        /// </summary>
        public string RoomNumber { get; set; } = string.Empty;

        /// <summary>
        /// 房型ID (Room Type ID)
        /// </summary>
        public int RoomTypeId { get; set; }

        /// <summary>
        /// 客户编号 (由服务端写入，客户端无需传递)
        /// </summary>
        public string CustomerNumber { get; set; } = string.Empty;

        /// <summary>
        /// 是否匿名
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// 本次入住唯一标识 (Stay ID)，由“我的入住”带入，用于服务端校验“一次入住仅可评价一次”
        /// </summary>
        public string StayId { get; set; } = string.Empty;

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
        /// 评价内容
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// 评价图片链接 (逗号分隔)
        /// </summary>
        public string PhotoUrls { get; set; } = string.Empty;
    }
}
