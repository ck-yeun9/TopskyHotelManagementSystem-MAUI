using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class ReadCustomerAccountInputDto
    {
        /// <summary>
        /// 账号 (Account)
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 密码 (Password)
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 邮箱 (Email)
        /// </summary>
        public string EmailAddress { get; set; }
    }
}
