using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace CET.Finance.Route.Common
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
        /// NLog配置文件路径
        /// </summary>
        public static readonly string NlogConfig = ConfigurationManager.AppSettings["NlogConfig"];
    }
}