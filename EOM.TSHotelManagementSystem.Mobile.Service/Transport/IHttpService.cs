using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IHttpService
    {
        /// <summary>
        /// 获取当前令牌（安全方式）
        /// </summary>
        Task<string> GetTokenAsync();

        /// <summary>
        /// 使用 RestSharp 上传文件（multipart/form-data）
        /// </summary>
        /// <param name="url">API地址</param>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="additionalParams">其他参数</param>
        /// <param name="dicHeaders">自定义Headers</param>
        /// <returns>响应结果</returns>
        Task<ResponseMsg> UploadFileAsync(
            string url,
            string filePath,
            Dictionary<string, string>? additionalParams = null,
            Dictionary<string, string>? dicHeaders = null);

        /// <summary>
        /// 统一请求方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        Task<ResponseMsg> RequestAsync(string url);

        /// <summary>
        /// 统一请求方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        Task<ResponseMsg> RequestAsync(string url, string? json = null);

        /// <summary>
        /// 统一请求方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        Task<ResponseMsg> RequestAsync(string url, Dictionary<string, string>? dic = null);

    }
}
