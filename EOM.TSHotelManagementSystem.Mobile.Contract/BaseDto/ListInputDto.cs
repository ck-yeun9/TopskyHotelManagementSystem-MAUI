namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class ListInputDto : BaseInputDto
    {
        /// <summary>
        /// 页数
        /// </summary>
        public int Page { get; set; } = 0;
        /// <summary>
        /// 总数
        /// </summary>
        public int PageSize { get; set; } = 10;
        /// <summary>
        /// 忽略分页
        /// </summary>
        public bool IgnorePaging { get; set; } = false;
    }
}
