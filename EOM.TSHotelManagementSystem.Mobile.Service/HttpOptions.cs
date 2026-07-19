namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    /// <summary>
    /// HTTP 请求相关配置。
    /// 由 UI 层从 appsettings.json 绑定并通过 DI 注入，Service 层不再写死。
    /// </summary>
    public class HttpOptions
    {
        /// <summary>
        /// WebApi 基地址，例如 https://host/api/（应含结尾的 "/"）。
        /// </summary>
        public string BaseUrl { get; set; } = "https://tshotel-debug.oscode.top/api/";

        /// <summary>
        /// 请求超时时间（秒），默认 30。
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// User-Agent 请求头，默认与历史实现保持一致。
        /// </summary>
        public string UserAgent { get; set; } =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36";
    }
}
