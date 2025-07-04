using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class HttpResponse<T>
    {
        public bool IsSuccess { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public T Data { get; private set; }
        public string ErrorMessage { get; private set; }
        public Exception Exception { get; private set; }
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        public static HttpResponse<T> Success(HttpStatusCode statusCode, T data)
        {
            return new HttpResponse<T>
            {
                IsSuccess = true,
                StatusCode = statusCode,
                Data = data
            };
        }

        public static HttpResponse<T> Failed(
            HttpStatusCode statusCode,
            string errorMessage,
            Exception exception = null)
        {
            return new HttpResponse<T>
            {
                IsSuccess = false,
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                Exception = exception
            };
        }
    }
}
