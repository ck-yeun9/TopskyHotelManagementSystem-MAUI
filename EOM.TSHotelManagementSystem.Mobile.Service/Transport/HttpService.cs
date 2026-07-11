using EOM.TSHotelManagementSystem.Mobile.Common.Utility;
using RestSharp;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    /// <summary>
    /// 文件上传帮助类
    /// </summary>
    public class HttpService:IHttpService
    {
#if DEBUG
        /// <summary>
        /// WebApi URL
        /// </summary>
        public static string apiUrl => DeviceInfo.Platform == DevicePlatform.Android
            ? "http://192.168.5.153:63001/api/"
            : "http://localhost:63001/api/";

#elif RELEASE
        /// <summary>
        /// WebApi URL
        /// </summary>
        //public const string apiUrl = "https://tshotel-api.oscode.top/api/";
#endif

        public class IgnoreNullValuesConverter : JsonConverter<object>
        {
            private readonly bool _convertEmptyStringToNull;

            public IgnoreNullValuesConverter(bool convertEmptyStringToNull = false)
            {
                _convertEmptyStringToNull = convertEmptyStringToNull;
            }

            public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            {
                if (value is null) return;

                var jsonDocument = JsonSerializer.SerializeToDocument(value, value.GetType(), options);
                var jsonElement = jsonDocument.RootElement;

                if (jsonElement.ValueKind != JsonValueKind.Object)
                {
                    JsonSerializer.Serialize(writer, value, options);
                    return;
                }

                writer.WriteStartObject();
                foreach (var prop in jsonElement.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Null) continue;

                    if (_convertEmptyStringToNull &&
                        prop.Value.ValueKind == JsonValueKind.String &&
                        string.IsNullOrEmpty(prop.Value.GetString()))
                    {
                        continue;
                    }

                    prop.WriteTo(writer);
                }
                writer.WriteEndObject();
            }

            public override bool CanConvert(Type typeToConvert)
            {
                return !typeToConvert.IsPrimitive &&
                       typeToConvert != typeof(string);
            }
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync(Constant.AccessTokenKey);

                if (!string.IsNullOrEmpty(token))
                    return token;

                token = Preferences.Get(Constant.AccessTokenKey, string.Empty);
                if (string.IsNullOrEmpty(token))
                    return string.Empty;

                // 从 Preferences 获取的是加密后的 token，需要解密
                try
                {
                    return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                }
                catch
                {
                    return token;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取令牌失败: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<ResponseMsg> UploadFileAsync(
            string url,
            string filePath,
            Dictionary<string, string>? additionalParams = null,
            Dictionary<string, string>? dicHeaders = null)
        {
            var sourceStr = url.Replace("​", string.Empty);

            var requestUrl = apiUrl + sourceStr;

            var client = new RestClient(requestUrl);
            var request = new RestRequest();


            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");

            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", $"Bearer {token}");
            }

            if (dicHeaders != null)
            {
                foreach (var key in dicHeaders.Keys)
                {
                    request.AddHeader(key, dicHeaders[key]);
                }
            }

            if (additionalParams != null)
            {
                foreach (var kv in additionalParams)
                {
                    request.AddParameter(kv.Key, kv.Value, ParameterType.GetOrPost);
                }
            }

            request.AddFile(
                name: "file",
                path: filePath,
                contentType: GetMimeType(filePath)
            );

            var response = client.ExecutePost(request);

            return new ResponseMsg
            {
                StatusCode = (int)response.StatusCode,
                Message = response.Content
            };
        }

        public async Task<ResponseMsg> RequestAsync(string url)
        {
            ResponseMsg msg = new ResponseMsg();

            //处理url
            var sourceStr = url.Replace("​", string.Empty);

            var requestUrl = apiUrl + sourceStr;

            if (!CheckNetworkStatus())
            {
                throw new Exception("网络连接不可用，请检查网络设置。");
            }

            msg = await DoGet(requestUrl);

            return msg;
        }

        public async Task<ResponseMsg> RequestAsync(string url, string? json = null)
        {
            ResponseMsg msg = new ResponseMsg();

            //处理url
            var sourceStr = url.Replace("​", string.Empty);

            var requestUrl = apiUrl + sourceStr;

            if (!CheckNetworkStatus())
            {
                throw new Exception("网络连接不可用，请检查网络设置。");
            }

            if (!string.IsNullOrEmpty(json))
            {
                msg = await DoPost(requestUrl, json);
            }
            else
            {
                msg = await DoGet(requestUrl);
            }

            return msg;
        }

        /// <returns></returns>
        public async Task<ResponseMsg> RequestAsync(string url, Dictionary<string, string>? dic = null)
        {
            ResponseMsg msg = new ResponseMsg();

            //处理url
            var sourceStr = url.Replace("​", string.Empty);

            var requestUrl = apiUrl + sourceStr;

            if (!CheckNetworkStatus())
            {
                throw new Exception("网络连接不可用，请检查网络设置。");
            }

            if (dic != null && dic.Count > 0)
            {
                msg = await DoGet(requestUrl, dic);
            }
            else
            {
                msg = await DoGet(requestUrl);
            }

            return msg;
        }

        private async Task<ResponseMsg> DoGet(string url, IDictionary<string, string>? parameters = null, string? contentType = null, string? referer = null, string? cookie = null, Dictionary<string, string>? dicHeaders = null)
        {
            if (parameters != null && parameters.Count > 0)
            {
                if (url.Contains("?"))
                {
                    url = url + "&" + BuildQuery(parameters);
                }
                else
                {
                    url = url + "?" + BuildQuery(parameters);
                }
            }

            var reponse = new RestResponse();
            var client = new RestClient(url);
            var request = new RestRequest();

            string? resultContent = null;
            RestResponse? rsp = null;

            try
            {
                if (!string.IsNullOrEmpty(referer))
                {
                    request.AddHeader("Referer", referer);
                }

                if (!string.IsNullOrEmpty(cookie))
                {
                    request.AddHeader("Cookie", cookie);
                }

                request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");

                if (dicHeaders != null)
                {
                    foreach (var key in dicHeaders.Keys)
                    {
                        request.AddHeader(key, dicHeaders[key]);
                    }
                }

                var token = await GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.AddHeader("Authorization", string.Format("Bearer {0}", token));
                }

                rsp = client.ExecuteGet(request);

                resultContent = rsp.Content;
            }
            catch (Exception)
            {
                throw;
            }

            return new ResponseMsg() { StatusCode = (int)rsp.StatusCode, Message = resultContent };
        }

        private async Task<ResponseMsg> DoPost(string url, string? jsonParam = null, string? contentType = null, string? referer = null, string? cookie = null, Dictionary<string, string>? dicHeaders = null)
        {
            var reponse = new RestResponse();
            var client = new RestClient(url);
            var request = new RestRequest();
            if (!string.IsNullOrEmpty(contentType))
            {
                request.AddHeader("Content-Type", contentType);
            }
            else
            {
                request.AddHeader("Content-Type", ContentType.Json);
            }

            if (!string.IsNullOrEmpty(referer))
            {
                request.AddHeader("Referer", referer);
            }

            if (!string.IsNullOrEmpty(cookie))
            {
                request.AddHeader("Cookie", cookie);
            }

            if (dicHeaders != null)
            {
                foreach (var key in dicHeaders.Keys)
                {
                    request.AddHeader(key, dicHeaders[key]);
                }
            }

            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");

            request.AddBody(jsonParam!);

            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", $"Bearer {token}");
            }

            reponse = client.ExecutePost(request);

            var responseString = reponse.Content;

            return new ResponseMsg() { StatusCode = (int)reponse.StatusCode, Message = responseString };
        }

        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        private string BuildQuery(IDictionary<string, string> parameters)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;

            IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }

                    postData.Append(name);
                    postData.Append("=");
                    postData.Append(UrlEncode(value, Encoding.UTF8));
                    hasParam = true;
                }
            }

            return postData.ToString();
        }

        private string? UrlEncode(string? str, Encoding e)
        {
            var REG_URL_ENCODING = new Regex(@"%[a-f0-9]{2}");

            if (str == null)
            {
                return null;
            }

            string stringToEncode = HttpUtility.UrlEncode(str, e).Replace("+", "%20").Replace("*", "%2A").Replace("(", "%28").Replace(")", "%29");
            return REG_URL_ENCODING.Replace(stringToEncode, m => m.Value.ToUpperInvariant());
        }

        private bool CheckNetworkStatus()
        {
#if DEBUG
            return Connectivity.NetworkAccess != NetworkAccess.None;
#else
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
#endif
        }
    }
}
