using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IHttpService
    {
        /// <summary>
        /// Gets the current auth token.
        /// </summary>
        Task<string> GetTokenAsync();

        /// <summary>
        /// Uploads a file with multipart/form-data.
        /// </summary>
        Task<ResponseMsg> UploadFileAsync(
            string url,
            string filePath,
            Dictionary<string, string>? additionalParams = null,
            Dictionary<string, string>? dicHeaders = null);

        /// <summary>
        /// Sends a GET request.
        /// </summary>
        Task<ResponseMsg> RequestAsync(string url);

        /// <summary>
        /// Sends a request with an optional JSON payload.
        /// </summary>
        Task<ResponseMsg> RequestAsync(string url, string? json = null);

        /// <summary>
        /// Sends a request with optional query parameters.
        /// </summary>
        Task<ResponseMsg> RequestAsync(string url, Dictionary<string, string>? dic = null);
    }
}
