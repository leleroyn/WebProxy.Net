using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Newtonsoft.Json;
using WebProxy.Models;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace WebProxy.Common
{
    public static class RouteHelper
    {
        private static readonly string RouteDataPath = Path.Combine(SettingsHelper.RootPath, "App_Data/route");
        private static readonly string RouteDataCacheKey = "routes.json";
        private static readonly string HostDataPath = Path.Combine(SettingsHelper.RootPath, "App_Data/servicehost");
        private static readonly string HostDataCacheKey = "hosts.json";

        /// <summary>
        /// 获取路由配置
        /// </summary>
        public static Dictionary<string, List<CustomRouteData>> GetRouteDatas()
        {
            var routeDic = HttpRuntime.Cache[RouteDataCacheKey] as Dictionary<string, List<CustomRouteData>>;
            if (routeDic == null)
            {
                //加载配置目录下所有的json文件
                string[] files = Directory.GetFiles(RouteDataPath, "*.json", SearchOption.AllDirectories);

                routeDic = new Dictionary<string, List<CustomRouteData>>();
                foreach (var file in files)
                {
                    var routeContent = File.ReadAllText(file);
                    var routeSet = JsonConvert.DeserializeObject<List<CustomRouteData>>(routeContent);

                    var singleDic = routeSet.GroupBy(o => o.Command).ToDictionary(
                          k => k.Key,
                          v => v.Select(o => o).ToList()
                         );

                    //跨配置文件Command必须保持唯一
                    foreach (var route in singleDic)
                        routeDic.Add(route.Key, route.Value);
                }

                CacheHelper.Set(RouteDataCacheKey, routeDic, files);
            }
            return routeDic;
        }

        /// <summary>
        /// 获取Host配置
        /// </summary>
        /// <returns></returns>
        public static List<ServiceHostData> GetHostDatas()
        {
            var hostDatas = HttpRuntime.Cache[HostDataCacheKey] as List<ServiceHostData>;
            if (hostDatas == null)
            {
                //加载配置目录下所有的json文件
                string[] files = Directory.GetFiles(HostDataPath, "*.json", SearchOption.AllDirectories);

                hostDatas = new List<ServiceHostData>();
                foreach (var file in files)
                {
                    var content = File.ReadAllText(file);
                    var data = JsonConvert.DeserializeObject<List<ServiceHostData>>(content);
                    hostDatas.AddRange(data);
                }

                CacheHelper.Set(HostDataCacheKey, hostDatas, files);
            }
            return hostDatas;
        }

        /// <summary>
        /// 路由负载均衡
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public static CustomRouteData RoutingLoadBalance(
            CustomRouteData routeData)
        {
            var route = new CustomRouteData
            {
                CacheCondition = routeData.CacheCondition,
                CacheTime = routeData.CacheTime,
                Command = routeData.Command,
                MicroService = routeData.MicroService,
                Name = routeData.Name,
                System = routeData.System,
                Version = routeData.Version
            };
            var hostData = GetHostDatas().FirstOrDefault(x => routeData.MicroService == x.Name);
            if (hostData != null)
            {
                var randomHost = RandomHelper.GetRandomList(hostData.Hosts.ToList(), 1).First();
                route.Handle = string.Concat(randomHost.ServiceUrl, routeData.Handle);
            }
            return route;
        }

        /// <summary>
        /// 获取最优路由
        /// </summary>
        /// <param name="command"></param>
        /// <param name="version"></param>
        /// <param name="system"></param>
        /// <returns></returns>
        public static CustomRouteData GetOptimalRoute(string command, string version, string system)
        {
            var routeDatas = GetRouteDatas();
            var routes = routeDatas.FirstOrDefault(x => string.Equals(x.Key, command, StringComparison.OrdinalIgnoreCase));
            if (routes.Value == null)
                return null;

            if (routes.Value.Count == 1)
                return routes.Value.First();

            IEnumerable<CustomRouteData> routeList = routes.Value;

            List<Expression<Func<CustomRouteData, bool>>> expressions = new List<Expression<Func<CustomRouteData, bool>>>();
            if (!string.IsNullOrEmpty(version))
            {
                expressions.Add(x => x.Version == version);
            }
            if (!string.IsNullOrEmpty(system))
            {
                expressions.Add(x => string.Equals(x.System.ToString(), system, StringComparison.OrdinalIgnoreCase));
            }

            routeList = expressions.Aggregate(routeList, (current, item) => current.Where(item.Compile()));

            if (routeList.Any())
            {
                return routeList.OrderBy(x => x.Version).ThenBy(x => x.System).FirstOrDefault();
            }

            return routes.Value
                    .Where(x => string.IsNullOrEmpty(x.Version) || x.System == SytemType.None)
                    .OrderBy(x => x.Version).ThenBy(x => x.System)
                    .FirstOrDefault();
        }

        public static string CreateToken(CustomRouteData route)
        {
            var host = GetHostDatas().First(o => o.Name == route.MicroService);
            IDateTimeProvider provider = new UtcDateTimeProvider();
            var now = provider.GetNow();
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); // or use JwtValidator.UnixEpoch
            var secondsSinceEpoch = Math.Round((now - unixEpoch).TotalSeconds) + 60; //60S后过期

            var payload = new Dictionary<string, object>
            {
            { "app_id", host.ApplicationId },
            { "exp", secondsSinceEpoch }
            };
            var secret = host.ApplicationKey;

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);
            return token;
        }
    }
}