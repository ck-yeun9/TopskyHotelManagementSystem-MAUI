using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static EOM.TSHotelManagementSystem.Mobile.Service.HttpService;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public static class HttpHelper
    {
        /// <summary>
        /// Json转数组列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="JsonStr"></param>
        /// <returns></returns>
        public static List<T>? JsonToList<T>(this string JsonStr)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };
            return JsonSerializer.Deserialize<List<T>>(JsonStr, options);
        }

        /// <summary>
        /// Json转分页列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T? JsonToPageList<T>(this string json) where T : class
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// Json转实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T? JsonToModel<T>(this string input)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };
            return JsonSerializer.Deserialize<T>(input, options);
        }

        /// <summary>
        /// 实体转Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ModelToJson<T>(this T input)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new IgnoreNullValuesConverter(true) },
                    WriteIndented = true
                };
                return JsonSerializer.Serialize(input, options);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}
