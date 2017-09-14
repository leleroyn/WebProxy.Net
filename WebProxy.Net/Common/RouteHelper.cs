using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Caching;
using Newtonsoft.Json;
using System.Web.Routing;
using WebProxy.Models;

namespace WebProxy.Common
{
    public static class RouteHelper
    {
        /// <summary>
        /// 获取路由配置
        /// </summary>        
        public static Dictionary<string, List<CustomRouteData>> RouteDatas
        {
            get
            {
                var routeDic = HttpRuntime.Cache["route.json"] as Dictionary<string, List<CustomRouteData>>;
                if (routeDic == null)
                {
                    var routePath = Path.Combine(SettingsHelper.RootPath, "App_Data");
                    string[] files = Directory.GetFiles(routePath, "*.json", SearchOption.AllDirectories);

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

                    CacheHelper.Set("route.json", routeDic, files);
                }
                return routeDic;
            }
        }

        /// <summary>
        /// 获取最优路由
        /// </summary>
        /// <param name="command"></param>
        /// <param name="version"></param>
        /// <param name="system"></param>
        /// <returns></returns>
        public static CustomRouteData GetOptimalRoute(string command,string version,string system)
        {
            var routes = RouteDatas.FirstOrDefault(x => string.Equals(x.Key, command, StringComparison.OrdinalIgnoreCase));
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

            if (!routeList.Any())
            {
                return routes.Value
                    .Where(x => string.IsNullOrEmpty(x.Version) || x.System == SytemType.None)
                    .OrderBy(x => x.Version).ThenBy(x => x.System)
                    .FirstOrDefault();
            }

            return routeList.OrderBy(x => x.Version).ThenBy(x => x.System).FirstOrDefault();
        }
    }
}