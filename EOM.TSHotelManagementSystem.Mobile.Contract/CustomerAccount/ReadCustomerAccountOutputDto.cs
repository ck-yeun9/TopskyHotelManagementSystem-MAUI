using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class ReadCustomerAccountOutputDto:BaseDto
    {
        /// <summary>
        /// 账号 (Account)
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 邮箱 (Email)
        /// </summary>
        public string EmailAddress { get; set; }
        /// <summary>
        /// 名称 (Name)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 密码 (Password)
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 状态 (Status)
        /// </summary>
        public int Status { get; set; } = 0;
        /// <summary>
        /// 最后一次登录地址 (Last Login IP)
        /// </summary>
        public string LastLoginIp { get; set; }
        /// <summary>
        /// 最后一次登录时间 (Last Login Time)
        /// </summary>
        public DateTime? LastLoginTime { get; set; }
        /// <summary>
        /// 刷新令牌 (Refresh Token)
        /// </summary>
        public string RefreshToken { get; set; }
    }
}