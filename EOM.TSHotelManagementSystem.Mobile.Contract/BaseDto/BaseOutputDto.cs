
namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class BaseOutputDto
    {
        /// <summary>
        /// 状态码，例如 200 表示成功，500 表示服务器错误，400 表示客户端错误等
        /// </summary>
        public int StatusCode { get; set; } = StatusCodeConstants.Success;

        /// <summary>
        /// 返回消息，用于描述请求结果
        /// </summary>
        public string Message { get; set; } = "成功";

        /// <summary>
        /// 
        /// </summary>
        public BaseOutputDto()
        {
            StatusCode = 200;
            Message = "成功";
        }

        /// <summary>
        /// 带状态码和消息的构造函数
        /// </summary>
        /// <param name="statusCode">状态码</param>
        /// <param name="message">消息</param>
        public BaseOutputDto(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
