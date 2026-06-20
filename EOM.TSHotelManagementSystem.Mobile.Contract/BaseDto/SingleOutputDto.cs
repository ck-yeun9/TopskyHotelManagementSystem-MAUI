namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class SingleOutputDto<T> : BaseOutputDto
    {
        /// <summary>
        /// 数据源
        /// </summary>
        public T Data { get; set; }
    }
}
