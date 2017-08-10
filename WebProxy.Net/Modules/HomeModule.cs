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
                 Dictionary<string, string> responseDic = new Dictionary<string, string>();

                 switch (HeadData.MultiRequestType)
                 {
                     //并行请求
                     case "parallel":
                         {
                             int i = 0;
                             Dictionary<string, Task> asyncResponseDic = new Dictionary<string, Task>();
                             foreach (var optimalRoute in OptimalRoutes)
                             {
                                 var cmd = optimalRoute.Key;
                                 var route = optimalRoute.Value;
                                 var body = BodyData == null ? null : BodyData[i];
                                 var requestResult = HandleRequest(cmd, HeadData, route, body);
                                 asyncResponseDic.Add(cmd, requestResult);
                                 i++;
                             }
                             var taskList = asyncResponseDic.Select(y => y.Value).ToArray();
                             await Task.WhenAll(taskList);

                             responseDic = asyncResponseDic.ToDictionary(z => z.Key, z => ((Task<string>)z.Value).Result);
                         }
                         break;
                     //串行请求
                     case "serial":
                     default:
                         {
                             int i = 0;
                             foreach (var optimalRoute in OptimalRoutes)
                             {
                                 var cmd = optimalRoute.Key;
                                 var route = optimalRoute.Value;
                                 var body = BodyData == null ? null : BodyData[i];
                                 var requestResult = await HandleRequest(cmd, HeadData, route, body);
                                 responseDic.Add(cmd, requestResult);
                                 i++;
                             }
                         }
                         break;
                 }

                 //单请求直接返回请求内容，多请求返回name-content的字典
                 if (responseDic.Count() == 1)
                 {
                     return responseDic.First().Value;
                 }

                 return responseDic;
             };
        }
    }
}