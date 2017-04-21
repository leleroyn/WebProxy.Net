using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using CET.Finance.Route.Common;
using JWT;
using Nancy;
using Newtonsoft.Json;
using RestSharp;

namespace CET.Finance.Route
{
    public class Demo
    {
        #region 客户端请求路由端

        /// <summary>
        /// 客户端请求路由端
        /// </summary>
        /// <returns></returns>
        public string ClientRequest()
        {
            //报文头
            dynamic head = new ExpandoObject();
            //流水号（必填）
            head.SerialNumber = Guid.NewGuid().ToString();
            //请求地址（必填）
            head.RequestHost = "127.0.0.1";
            //请求时间（必填）
            head.RequestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //命名名（必填）
            head.Command = "Finance.FinanceList";
            //版本号（选填）
            head.Version = "1.2.0";
            //系统类型【pc，android,ios】（选填）
            head.System = "pc";
            //渠道【web,wap,app,cache】（选填）
            head.Channel = "web";
            //用户ID（选填）
            head.UserId = "";
            //是否使用缓存（选填）
            head.UseCache = "false";
            string headData = JsonConvert.SerializeObject(head);
            headData = JsonWebToken.Base64UrlEncode(Encoding.UTF8.GetBytes(headData));

            //Body参数，不同接口传递不同参数，可空
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            bodyDic.Add("Duration", -1);
            bodyDic.Add("Interest", -1);
            bodyDic.Add("PageIndex", 1);
            bodyDic.Add("PageSize", 10);

            //Body使用DES加密，key参考GetDesKey方法
            string bodyData = JsonConvert.SerializeObject(bodyDic);
            bodyData = JsonWebToken.Base64UrlEncode(Encoding.UTF8.GetBytes(bodyData));
            string encryptBody = EncryptHelper.DESEncrypt(bodyData, GetDesKey(head.Channel));

            string url = "http://localhost:6854/Api";
            //if (url.ToLower().Contains("https"))
            //{
            //    //忽略https校验
            //    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            //}

            //-- Post
            RestClient client = new RestClient(url);
            client.Proxy = null;
            client.Timeout = 60000;
            client.MaxRedirects = 1;
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("head", headData);
            request.AddParameter("body", encryptBody);
            string result = client.Execute(request).Content;
            return result;
        }

        /// <summary>
        /// 获取验签密钥
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetDesKey(string key)
        {
            //<WebDesKey>
            //    <add key="web" value="501fe9ec9c4befd0d2a558adc3fc80fe" />
            //    <add key="app" value="6b47bac49690d5b5b2a8167e2ae57860" />
            //    <add key="wap" value="7389D38180B0424293115732F6998833" />
            //    <add key="cache" value="cded3dda4367e4239544584e4411b1ed" />
            //</WebDesKey>

            string originalKey = "501fe9ec9c4befd0d2a558adc3fc80fe";
            string md5 = EncryptHelper.GetMd5Hash(originalKey);

            return md5.Substring(0, 8);
        }

        #endregion //客户端请求路由端

        #region 微服务接收到路由端的转发
        /// <summary>
        /// 微服务接收到路由端的转发
        /// </summary>
        public void WebAPIReceive()
        {
            //注意：路由转发的请求暂不加密也不使用签名

            ////- Head
            //var headFrom = Request.Headers["head"].FirstOrDefault();
            //if (headFrom != null)
            //{
            //    headFrom = Encoding.UTF8.GetString(JWT.JsonWebToken.Base64UrlDecode(headFrom));
            //    var headData = JsonConvert.DeserializeObject<HeadModel>(headFrom);
            //}

            ////- Body
            //var bodyForm = Request.Form["body"];
            //if (bodyForm != null)
            //{
            //    bodyForm = Encoding.UTF8.GetString(JWT.JsonWebToken.Base64UrlDecode(bodyForm));
            //    var bodyData = JsonConvert.DeserializeObject<Dictionary<string, object>>(bodyForm);
            //}

            //do something
        }

        public class HeadModel
        {
            /// <summary>
            /// 流水号
            /// </summary>
            public string SerialNumber { get; set; }
            /// <summary>
            /// 请求地址
            /// </summary>
            public string RequestHost { get; set; }
            /// <summary>
            /// 请求时间
            /// </summary>
            public string RequestTime { get; set; }
            /// <summary>
            /// 渠道
            /// </summary>
            public string Channel { get; set; }
            /// <summary>
            /// 用户ID
            /// </summary>
            public string UserId { get; set; }
        }

        #endregion //微服务接收到路由端的转发
    }
}