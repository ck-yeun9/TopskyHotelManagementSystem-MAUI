
namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class BaseOutputDto
    {
        /// <summary>
        /// 业务状态码，0 表示成功
        /// </summary>
        public int Code { get; set; } = 0;

        /// <summary>
        /// 返回消息，用于描述请求结果
        /// </summary>
        public string Message { get; set; } = "成功";

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success => Code == 0;

        /// <summary>
        /// 
        /// </summary>
        public BaseOutputDto()
        {
            Code = 0;
            Message = "成功";
        }

        /// <summary>
        /// 带状态码和消息的构造函数
        /// </summary>
        /// <param name="code">业务状态码</param>
        /// <param name="message">消息</param>
        public BaseOutputDto(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
