using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace WebProxy.Net.Common
{
    public class Settings
    {
        #region 验签密钥

        /// <summary>
        /// 验签密钥配置
        /// </summary>
        public static NameValueCollection WebDesKeys
        {
            get
            {
                return (NameValueCollection)ConfigurationManager.GetSection("webDesKey");
            }
        }

        /// <summary>
        /// 获取验签密钥
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetDesKey(string key)
        {
            return WebDesKeys[key.ToLower()];

            //string originalKey = WebDesKeys[key.ToLower()];
            //string md5 = EncryptHelper.GetMd5Hash(originalKey);

            //return md5.Substring(0, 8);
        }

        #endregion //验签密钥

        /// <summary>
        /// 是否忽略日志
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static bool IgnoreLogChannel(string channel)
        {
            string ignoreLogChannel = ConfigurationManager.AppSettings["IgnoreLogChannel"];
            if (string.IsNullOrWhiteSpace(ignoreLogChannel)) 
                return false;
            return ignoreLogChannel.Split(',').Any(o => o.Equals(channel, StringComparison.OrdinalIgnoreCase));
        }
 
    }
}