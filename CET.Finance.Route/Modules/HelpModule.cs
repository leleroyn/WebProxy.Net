using System;
using CET.Finance.Route.Common;
using Nancy;
using Newtonsoft.Json;

namespace CET.Finance.Route.Modules
{
    public class HelpModule : NancyModule
    {
        public HelpModule()
        {
            Get["/"] = _ =>
            {
                return string.Format("Server Time：{0}", DateTime.Now);
            };

            Get["/Help"] = _ =>
            {
                var routeDic = RouteHelper.RouteDatas;
                return JsonConvert.SerializeObject(routeDic, Formatting.Indented);
            };
        }
    }
}