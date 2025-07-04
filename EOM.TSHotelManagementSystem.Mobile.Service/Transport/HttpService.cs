using EOM.TSHotelManagementSystem.Mobile.Contract;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class HttpService : IHttpService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        private static string Api = "https://tshotel-debug.oscode.top/api/";

        public HttpService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<SingleOutputDto<ReadCustomerAccountOutputDto>> CustomerAuthorizationRequestAsync<T>(
            string endpoint,
            object body = null)
        {
            if (!CheckNetworkStatus())
                throw new Exception("Not Network");

            var client = new RestClient($"{GetBaseUrl()}{endpoint}");

            var request = new RestRequest();

            request.AddHeader("Content-Type", ContentType.Json);

            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");

            request.AddBody(body!);

            RestResponse response = client.ExecutePost(request);
            if (response.IsSuccessful)
            {
                var responseData = response.Content;
                if (!string.IsNullOrEmpty(responseData))
                {
                    var result = JsonSerializer.Deserialize<SingleOutputDto<ReadCustomerAccountOutputDto>>(responseData);
                    return result;
                }
            }
            return null;
        }

        private bool CheckNetworkStatus()
        {
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }

        private string GetBaseUrl()
        {
            return Api;
        }
    }
}
