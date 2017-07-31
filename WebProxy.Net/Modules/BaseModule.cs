using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using WebProxy.Common;
using Nancy;
using Newtonsoft.Json;

namespace WebProxy.Modules
{
    public class BaseModule : NancyModule
    {
        protected RequestHead HeadData;
        protected string HeadOriginalStr;
        protected Dictionary<string, object> BodyData;
        protected RouteData OptimalRoute;
        private bool _useCache;

        public BaseModule()
        {
            DateTime elapsedTime = DateTime.Now;
            bool ignoreLog = false;
            Before += ctx =>
            {
                GetRequestData(ctx.Request);
                ignoreLog = Settings.IgnoreLogChannel(HeadData.Channel);
                VerifyData(HeadData, OptimalRoute);
                return null;
            };

            After += ctx =>
            {
                if (!ignoreLog)
                {
                    string response;
                    using (MemoryStream respData = new MemoryStream())
                    {
                        ctx.Response.Contents(respData);
                        response = Encoding.UTF8.GetString(respData.ToArray());
                    }

                    LogHelper.Info(HeadData.Command,
                        string.Format(
                            "Route request successfully,Address:{0},Time:{1}(s),Head:{2},Body:{3},RouteData:{4},Response:{5},UseCache:{6}",
                            Request.Url, (DateTime.Now - elapsedTime).TotalSeconds, HeadOriginalStr,
                            JsonConvert.SerializeObject(BodyData), JsonConvert.SerializeObject(OptimalRoute), response,
                            _useCache));
                }
            };

            OnError += (ctx, ex) =>
            {
                if (!ignoreLog)
                {
                    LogHelper.Error(
                        string.Format("Route request Error,Command[{0}]", HeadData == null ? "" : HeadData.Command),
                        string.Format(
                            "Route request error,Address:{0},End time:{1},Head:{2},Body:{3},RouteData:{4},Error Message:{5}",
                            Request.Url, DateTime.Now, HeadOriginalStr, JsonConvert.SerializeObject(BodyData),
                            JsonConvert.SerializeObject(OptimalRoute), ex.Message), ex);
                }
                dynamic response = new ExpandoObject();
                response.Code = "500";
                response.ErrorMessage = string.Format("Route请求异常，Message:{0}", ex.Message);
                return JsonConvert.SerializeObject(response);
            };
        }

        /// <summary>
        /// 判断是否使用缓存
        /// </summary>
        /// <returns></returns>
        protected bool UseCache
        {
            get
            {
                //启用缓存条件
                //- 请求Head参数UserCache:true
                //- 路由缓存时间配置大于0
                //- 渠道不为null，且渠道不在忽略的列表（IgnoreCacheChannel）中
                //- 满足请求条件，满足其一即可：
                //-- 请求Body无参数且路由缓存条件不存在
                //-- 请求body含参数且路由缓存存在条件且请求body所有非空字段都包含在路由缓存条件中
                if (!string.IsNullOrEmpty(HeadData.UseCache)
                    && HeadData.UseCache.ToLower() == "true"
                    && OptimalRoute.CacheTime != 0
                    && !IsIgnoreCache())
                {
                    _useCache |= ((BodyData == null && OptimalRoute.CacheCondition == null)
                        || (BodyData != null && OptimalRoute.CacheCondition != null && BodyData.Count(x => x.Value != null) == BodyData.Count(x => OptimalRoute.CacheCondition.ContainsKey(x.Key)) && BodyData.Count(x => x.Value != null) == OptimalRoute.CacheCondition.Count(x => x.Value.Contains(BodyData.First(y => string.Equals(y.Key, x.Key, StringComparison.OrdinalIgnoreCase)).Value.ToString()))));
                }
                return _useCache;
            }
            set
            {
                _useCache = value;
            }
        }

        /// <summary>
        /// 生成缓存Key
        /// </summary>
        /// <returns></returns>
        protected string GeneralCacheKey()
        {
            string key = string.Join("_", HeadData.Command, HeadData.Version, HeadData.System);
            if (OptimalRoute.CacheCondition != null)
            {
                List<string> userCondition = new List<string>();
                foreach (var condition in OptimalRoute.CacheCondition)
                {
                    if (BodyData != null && BodyData.Keys.Contains(condition.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        var bodyVal = BodyData.First(x => string.Equals(x.Key, condition.Key, StringComparison.OrdinalIgnoreCase)).Value.ToString();
                        if (condition.Value.Contains(bodyVal))
                        {
                            string val = string.Format("{0}={1}", condition.Key, bodyVal);
                            userCondition.Add(val);
                        }
                    }
                }
                if (userCondition.Count > 0)
                {
                    key = string.Join("_", key, string.Join("_", userCondition.ToArray()));
                }
            }
            return key.ToLower();
        }

        //--------------------------

        /// <summary>
        /// 获取请求信息
        /// </summary>
        private void GetRequestData(Request request)
        {
            //- Head
            var head = request.Headers["head"].FirstOrDefault();
            if (head == null)
            {
                throw new Exception("请求报文头数据不存在或格式不正确");
            }
            HeadOriginalStr = Encoding.UTF8.GetString(EncodingHelper.Base64UrlDecode(head));
            HeadData = JsonConvert.DeserializeObject<RequestHead>(HeadOriginalStr);
            //- Body
            var bodyForm = request.Form["body"];
            if (bodyForm != null)
            {
                string key = Settings.GetDesKey(HeadData.Channel);
                bodyForm = EncryptHelper.DESDecrypt(bodyForm, key);
                bodyForm = Encoding.UTF8.GetString(EncodingHelper.Base64UrlDecode(bodyForm));
                BodyData = JsonConvert.DeserializeObject<Dictionary<string, object>>(bodyForm);
            }
            //- Route
            OptimalRoute = RouteHelper.GetOptimalRoute(HeadData);
        }

        /// <summary>
        /// 校验数据
        /// </summary>
        /// <param name="head"></param>  
        /// <param name="route"></param>
        private void VerifyData(RequestHead head, RouteData route)
        {
            if (head == null)
                throw new ArgumentNullException(nameof(head), "请求报文头数据不存在");

            if (route == null)
                throw new ArgumentNullException(nameof(route), "请求路由不存在");
        }

        /// <summary>
        /// 是否忽略缓存
        /// </summary>
        /// <returns></returns>
        private bool IsIgnoreCache()
        {
            if (!string.IsNullOrEmpty(HeadData.Channel)
                && !Settings.IgnoreCacheChannel.Contains(HeadData.Channel.ToLower()))
                return false;

            return true;
        }
    }
}