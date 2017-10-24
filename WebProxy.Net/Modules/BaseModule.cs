using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebProxy.Common;
using Nancy;
using Newtonsoft.Json;
using WebProxy.Models;

namespace WebProxy.Modules
{
    public class BaseModule : NancyModule
    {
        protected RequestHead HeadData;
        protected Dictionary<string, object> BodyData;
        protected CustomRouteData OptimalRoute;
        protected bool FinalUseCache;

        public BaseModule()
        {
            DateTime elapsedTime = DateTime.Now;
            bool ignoreLog = false;
            Before += ctx =>
            {
                var route = GetRequestData(ctx.Request);
                OptimalRoute = route;
                HeadData.Authorization = RouteHelper.CreateToken(route);
                ignoreLog = SettingsHelper.IgnoreLogChannel(HeadData.Channel);
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
                            Request.Url, (DateTime.Now - elapsedTime).TotalSeconds, JsonConvert.SerializeObject(HeadData),
                            JsonConvert.SerializeObject(BodyData), JsonConvert.SerializeObject(OptimalRoute), response,
                            FinalUseCache));
                }
            };

            OnError += (ctx, ex) =>
            {
                if (!ignoreLog)
                {
                    LogHelper.Error(
                        string.Format("Route request Error,Command{0}", HeadData == null ? "" : HeadData.Command),
                        string.Format(
                            "Route request error,Address:{0},End time:{1},Head:{2},Body:{3},RouteData:{4},Error Message:{5}",
                            Request.Url, DateTime.Now, JsonConvert.SerializeObject(HeadData), JsonConvert.SerializeObject(BodyData),
                            JsonConvert.SerializeObject(OptimalRoute), ex.Message), ex);
                }
                dynamic response = new ExpandoObject();
                response.Code = "500";
                response.ErrorMessage = string.Format("Route请求异常，Message:{0}", ex.Message);
                return JsonConvert.SerializeObject(response);
            };
        }

        /// <summary>
        /// 校验是否启用缓存
        /// </summary>
        /// <param name="userCache">请求启用缓存字段</param>
        /// <param name="channel">请求渠道</param>
        /// <param name="route">最优路由</param>
        /// <param name="body">请求参数</param>
        /// <returns></returns>
        protected bool CheckUseCache(bool? userCache, string channel, CustomRouteData route, Dictionary<string, object> body)
        {
            //启用缓存条件
            //- 请求Head参数UserCache:true
            //- 路由缓存时间配置大于0
            //- 渠道不为null，且渠道不在忽略的列表（IgnoreCacheChannel）中
            //- 满足请求条件，满足其一即可：
            //-- 请求Body无参数且路由缓存条件不存在
            //-- 请求body含参数且路由缓存存在条件且请求body所有非空字段都包含在路由缓存条件中           

            if (!userCache.HasValue || userCache.Value == false)
                return false;

            if (SettingsHelper.IgnoreCacheChannel(channel))
                return false;
            if (route.CacheTime == 0)
                return false;

            if (body == null || body.Count == 0)
                return true;

            if (route.CacheCondition == null)
                return false;

            List<Tuple<string, bool>> parms = new List<Tuple<string, bool>>();
            foreach (var p in body)
            {
                if (p.Value != null)
                {
                    string cValue = string.Empty;
                    if (!route.CacheCondition.TryGetValue(p.Key, out cValue))
                    {
                        cValue = string.Empty;
                    }
                    var parm = Tuple.Create(p.Key, cValue.Split(',').Contains(p.Value.ToString()));
                    parms.Add(parm);
                }
            }
            if (parms.Count(o => o.Item2 == true) == parms.Count) return true;

            return false;
        }

        /// <summary>
        /// 生成缓存Key
        /// </summary>
        /// <param name="command">请求命令</param>
        /// <param name="version">请求版本</param>
        /// <param name="system">请求系统</param>
        /// <param name="route">最优路由</param>
        /// <param name="body">请求参数</param>
        /// <returns></returns>
        protected string GeneralCacheKey(string command, string version, string system, CustomRouteData route, Dictionary<string, object> body)
        {
            // 缓存key格式:
            // home.banner_1.0.0_pc_condition1=value1_condition2=value2

            string key = string.Join("_", command, version, system);
            if (route.CacheCondition != null)
            {
                List<string> userCondition = new List<string>();
                foreach (var condition in route.CacheCondition)
                {
                    if (body != null && body.Keys.Contains(condition.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        var bodyVal = body.First(x => string.Equals(x.Key, condition.Key, StringComparison.OrdinalIgnoreCase)).Value.ToString();
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

        /// <summary>
        /// 请求处理
        /// </summary>
        /// <param name="command">请求指令</param>
        /// <param name="head">请求报文头</param>
        /// <param name="route">最优路由</param>
        /// <param name="body">请求参数</param>
        /// <returns></returns>
        protected async Task<string> HandleRequest(string command, RequestHead head, CustomRouteData route, Dictionary<string, object> body)
        {
            string response;
            // 根据请求参数判断是否启用缓存
            // 启用-生成缓存KEY,并尝试读取缓存，成功则返回缓存值，失败则转发请求
            // 不启用-转发请求
            bool isUseCache = CheckUseCache(head.UseCache, head.Channel, route, body);
            if (isUseCache)
            {
                string key = GeneralCacheKey(command, head.Version, head.System, route, body);
                var cacheValue = CacheHelper.Get(key);
                if (cacheValue != null)
                {
                    response = cacheValue;
                    FinalUseCache = true;
                }
                else
                {
                    response = await HttpClient.PostAsync(route.Handle, head, body);
                    CacheHelper.Set(key, response, new TimeSpan(0, 0, route.CacheTime));
                    FinalUseCache = false;
                }
            }
            else
            {
                response = await HttpClient.PostAsync(route.Handle, head, body);
                FinalUseCache = false;
            }

            return response;
        }


        /// <summary>
        /// 获取请求信息
        /// </summary>
        private CustomRouteData GetRequestData(Request request)
        {
            //- Head
            var head = request.Headers["head"].FirstOrDefault();
            if (head == null)
            {
                throw new Exception("请求报文头数据不存在或格式不正确");
            }
            head = Encoding.UTF8.GetString(EncodingHelper.Base64UrlDecode(head));
            HeadData = JsonConvert.DeserializeObject<RequestHead>(head);
            if (HeadData == null)
                throw new Exception("请求报文头数据不存在");
            if (string.IsNullOrEmpty(HeadData.Command))
                throw new Exception("请求报文头指令名称不能为空");
            //- Body
            var bodyForm = request.Form["body"];
            if (!string.IsNullOrWhiteSpace(bodyForm))
            {
                string key = SettingsHelper.GetDesKey(HeadData.Channel);
                bodyForm = EncryptHelper.DESDecrypt(bodyForm, key);
                string bodyStr = Encoding.UTF8.GetString(EncodingHelper.Base64UrlDecode(bodyForm));
                BodyData = JsonConvert.DeserializeObject<Dictionary<string, object>>(bodyStr);
            }
            //- Route
            CustomRouteData route = RouteHelper.GetOptimalRoute(HeadData.Command, HeadData.Version, HeadData.System);
            if (route == null)
                throw new Exception("请求路由不存在");

            route = RouteHelper.RoutingLoadBalance(route);

            return route;
        }
    }
}