using EOM.TSHotelManagementSystem.Mobile.Common.Utility;
using RestSharp;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    /// <summary>
    /// 文件上传帮助类
    /// </summary>
    public class HttpService:IHttpService
    {
        private readonly HttpOptions _httpOptions;

        /// <summary>
        /// WebApi 基地址（来自 appsettings.json，由 DI 注入，不再写死）。
        /// 始终以 "/" 结尾，便于与后续路径直接拼接。
        /// </summary>
        private string BaseUrl => _httpOptions.BaseUrl.EndsWith("/")
            ? _httpOptions.BaseUrl
            : _httpOptions.BaseUrl + "/";

        public HttpService(HttpOptions httpOptions)
        {
            _httpOptions = httpOptions;
        }

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

            var requestUrl = BaseUrl + sourceStr;

            var client = new RestClient(requestUrl);
            var request = new RestRequest();


            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddHeader("User-Agent", _httpOptions.UserAgent);
            request.Timeout = TimeSpan.FromSeconds(_httpOptions.TimeoutSeconds);

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

            HttpLogger.LogRequest("POST(multipart)", requestUrl, body: $"file={filePath}", hasToken: !string.IsNullOrEmpty(token));

            var response = client.ExecutePost(request);

            HttpLogger.LogResponse((int)response.StatusCode, response.Content);

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

            var requestUrl = BaseUrl + sourceStr;

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

            var requestUrl = BaseUrl + sourceStr;

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

            var requestUrl = BaseUrl + sourceStr;

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

                request.AddHeader("User-Agent", _httpOptions.UserAgent);
                request.Timeout = TimeSpan.FromSeconds(_httpOptions.TimeoutSeconds);

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

                HttpLogger.LogRequest("GET", url, body: null, hasToken: !string.IsNullOrEmpty(token));

                rsp = client.ExecuteGet(request);

                HttpLogger.LogResponse((int)rsp.StatusCode, rsp.Content);

                resultContent = rsp.Content;
            }
            catch (Exception ex)
            {
                HttpLogger.LogException("GET " + url, ex);
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

            request.AddHeader("Idempotency-Key", Guid.NewGuid().ToString());

            request.AddHeader("User-Agent", _httpOptions.UserAgent);
            request.Timeout = TimeSpan.FromSeconds(_httpOptions.TimeoutSeconds);

            request.AddBody(jsonParam!);

            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", $"Bearer {token}");
            }

            HttpLogger.LogRequest("POST", url, body: jsonParam, hasToken: !string.IsNullOrEmpty(token));

            try
            {
                reponse = client.ExecutePost(request);
            }
            catch (Exception ex)
            {
                HttpLogger.LogException("POST " + url, ex);
                throw;
            }

            HttpLogger.LogResponse((int)reponse.StatusCode, reponse.Content);

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
