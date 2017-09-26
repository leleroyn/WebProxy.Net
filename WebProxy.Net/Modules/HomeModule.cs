using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Newtonsoft.Json;

namespace WebProxy.Modules
{
    public class HomeModule : BaseModule
    {
        public HomeModule()
        {
             Post["/Api", true] = async (x, ct) =>
             {
                 var cmd = OptimalRoute.Command;
                 var body = BodyData == null ? null : BodyData;
                 var requestResult = await HandleRequest(cmd, HeadData, OptimalRoute, body);
                 return requestResult;
             };
        }
    }
}