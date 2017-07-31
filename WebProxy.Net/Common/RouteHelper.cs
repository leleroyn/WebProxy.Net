using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Caching;
using Newtonsoft.Json;

namespace WebProxy.Net.Common
{
    public class RouteHelper
    {
        /// <summary>
        /// 获取路由配置
        /// </summary>        
        public static Dictionary<string, List<RouteData>> RouteDatas
        {
            get
            {
                var routeDic = HttpRuntime.Cache["route.json"] as Dictionary<string, List<RouteData>>;
                if (routeDic == null)
                {
                    var routePath = Path.Combine(Settings.RootPath, "App_Data");
                    string[] files = Directory.GetFiles(routePath, "*.json", SearchOption.AllDirectories);

                    routeDic = new Dictionary<string, List<RouteData>>();
                    foreach (var file in files)
                    {
                        var routeContent = File.ReadAllText(file);
                        var routeSet = JsonConvert.DeserializeObject<List<RouteData>>(routeContent);

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
        /// <param name="head"></param>
        /// <returns></returns>
        public static RouteData GetOptimalRoute(RequestHead head)
        {
            var routes = RouteDatas.FirstOrDefault(x => string.Equals(x.Key, head.Command, StringComparison.OrdinalIgnoreCase));
            if (routes.Value == null)
                return null;

            if (routes.Value.Count == 1)
                return routes.Value.First();

            IEnumerable<RouteData> routeList = routes.Value;

            List<Expression<Func<RouteData, bool>>> expressions = new List<Expression<Func<RouteData, bool>>>();
            if (!string.IsNullOrEmpty(head.Version))
            {
                expressions.Add(x => x.Version == head.Version);
            }
            if (!string.IsNullOrEmpty(head.System))
            {
                expressions.Add(x => string.Equals(x.System.ToString(), head.System, StringComparison.OrdinalIgnoreCase));
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