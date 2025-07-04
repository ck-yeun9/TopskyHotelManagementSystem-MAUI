using EOM.TSHotelManagementSystem.Mobile.Contract;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IHttpService
    {
        Task<SingleOutputDto<ReadCustomerAccountOutputDto>> CustomerAuthorizationRequestAsync<T>(
            string endpoint,
            object body = null);
    }
}
