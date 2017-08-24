using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace WebProxy.Common
{
    public static class Settings
    {
        /// <summary>
        /// 获取验签密钥
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetDesKey(string key)
        {
            NameValueCollection webDesKeys = (NameValueCollection)ConfigurationManager.GetSection("webDesKey");

            return webDesKeys[key.ToLower()];

            //string originalKey = WebDesKeys[key.ToLower()];
            //string md5 = EncryptHelper.GetMd5Hash(originalKey);
            //return md5.Substring(0, 8);
        }

        /// <summary>
        /// 是否忽略日志
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static bool IgnoreLogChannel(string channel)
        {
            string ignoreLogChannel = ConfigurationManager.AppSettings["ignoreLogChannel"];
            if (string.IsNullOrWhiteSpace(ignoreLogChannel))
                return false;
            return ignoreLogChannel.Split(',').Any(o => o.Equals(channel, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 是否忽略缓存
        /// </summary>
        /// <returns></returns>
        public static bool IgnoreCacheChannel(string channel)
        {
            string[] ignoreCacheChannel = (ConfigurationManager.AppSettings["ignoreCacheChannel"] ?? "").ToLower().Split(',');
            if (!string.IsNullOrEmpty(channel)
                && !ignoreCacheChannel.Contains(channel.ToLower()))
                return false;
            return true;
        }

        /// <summary>
        /// 程序根目录
        /// </summary>
        public static string RootPath { get; set; }

        /// <summary>
        /// 多指令请求分隔字符
        /// </summary>
        public static char MultiCommandSplitChar = '|';
    }
}