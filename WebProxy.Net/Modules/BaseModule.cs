using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using WebProxy.Net.Common;
using Nancy;
using Newtonsoft.Json;

namespace WebProxy.Net.Modules
{
    public class BaseModule : NancyModule
    {
        protected RequestHead HeadData;
        protected string HeadOriginalStr;
        protected Dictionary<string, object> BodyData;
        protected RouteData OptimalRoute;
        private bool _useCache = false;

        public BaseModule()
        {
            DateTime elapsedTime = DateTime.Now;
            bool ignoreLog = false;
            Before += ctx =>
            {
                GetRequestData(ctx.Request);
                ignoreLog = Settings.IgnoreLogChannel(HeadData.Channel);
                VerifyData(HeadData, BodyData, OptimalRoute);
                return null;
            };

            After += ctx =>
            {
                if (!ignoreLog)
                {
                    LogHelper.Info(HeadData.Command, string.Format("Route request successfully,Time:{0}(s),Head:{1},Body:{2},RouteData:{3},UseCache:{4}", (DateTime.Now - elapsedTime).TotalSeconds, HeadOriginalStr, JsonConvert.SerializeObject(BodyData), JsonConvert.SerializeObject(OptimalRoute), _useCache));

                }
            };

            OnError += (ctx, ex) =>
            {
                if (!ignoreLog)
                {
                    LogHelper.Error(string.Format("Route request Error,Command[{0}]", HeadData == null ? "" : HeadData.Command), string.Format("Route request error,End time:{0},Head:{1},Body:{2},RouteData:{3},Error Message:{4}", DateTime.Now, HeadOriginalStr, JsonConvert.SerializeObject(BodyData), JsonConvert.SerializeObject(OptimalRoute), ex.Message), ex);

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
                if (!string.IsNullOrEmpty(HeadData.UseCache) && HeadData.UseCache.ToLower() == "true"
                    && OptimalRoute.CacheTime != 0)
                {
                    if (BodyData == null && OptimalRoute.CacheCondition == null)
                    {
                        _useCache = true;
                    }
                    else if ((BodyData != null && OptimalRoute.CacheCondition != null &&
                              BodyData.Count(x => x.Value != null) == OptimalRoute.CacheCondition.Count))
                    {
                        if (BodyData.Count(x => x.Value != null) ==
                            BodyData.Count(x => OptimalRoute.CacheCondition.ContainsKey(x.Key)))
                        {
                            if (BodyData.Count(x => x.Value != null) == OptimalRoute.CacheCondition.Count(
                                x => x.Value.Contains(
                                        BodyData.First(
                                            y => string.Equals(y.Key, x.Key, StringComparison.OrdinalIgnoreCase))
                                            .Value.ToString())))
                            {
                                _useCache = true;
                            }                           
                        }                        
                    }                    
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
            HeadOriginalStr = Encoding.UTF8.GetString(JWT.JsonWebToken.Base64UrlDecode(head));
            HeadData = JsonConvert.DeserializeObject<RequestHead>(HeadOriginalStr);
            //- Body
            var bodyForm = request.Form["body"];
            if (bodyForm != null)
            {
                string key = Settings.GetDesKey(HeadData.Channel);
                bodyForm = EncryptHelper.DESDecrypt(bodyForm, key);
                bodyForm = Encoding.UTF8.GetString(JWT.JsonWebToken.Base64UrlDecode(bodyForm));
                BodyData = JsonConvert.DeserializeObject<Dictionary<string, object>>(bodyForm);
            }
            //- Route
            OptimalRoute = RouteHelper.GetOptimalRoute(HeadData);
        }

        /// <summary>
        /// 校验数据
        /// </summary>
        /// <param name="head"></param>
        /// <param name="body"></param>
        /// <param name="route"></param>
        private void VerifyData(RequestHead head, Dictionary<string, object> body, RouteData route)
        {
            if (head == null)
                throw new ArgumentNullException("Head", "请求报文头数据不存在");
            if (route == null)
                throw new ArgumentNullException("Route", "请求路由不存在");
        }
    }
}