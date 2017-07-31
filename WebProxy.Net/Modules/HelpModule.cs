using System;
using WebProxy.Common;
using Nancy;
using Newtonsoft.Json;

namespace WebProxy.Modules
{
    public class HelpModule : NancyModule
    {
        public HelpModule()
        {
            Get["/"] = _ =>
            {
                return string.Format("Server Time:{0}", DateTime.Now);
            };

            Get["/Help"] = _ =>
            {
                var routeDic = RouteHelper.RouteDatas;
                return JsonConvert.SerializeObject(routeDic, Formatting.Indented);
            };
        }
    }
}