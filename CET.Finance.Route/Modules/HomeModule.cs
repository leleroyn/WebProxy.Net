using System;
using System.Collections.Generic;
using System.Linq;
using CET.Finance.Route.Common;
using Newtonsoft.Json;

namespace CET.Finance.Route.Modules
{
    public class HomeModule : BaseModule
    {
        public HomeModule()
        {
            Post["/Api",true] = async (x, ct) =>
            {
                string result;
                if (UseCache)
                {
                    //读取缓存
                    string key = GeneralCacheKey();
                    var cacheValue = CacheHelper.Get(key);
                    if (cacheValue != null)
                    {
                        result = cacheValue;
                    }
                    else
                    {
                        string postResult = await HttpClient.PostAsync(OptimalRoute.Handle, HeadData, BodyData);
                        result = postResult;

                        CacheHelper.Set(key, postResult, new TimeSpan(0, 0, OptimalRoute.CacheTime)); 
                        UseCache = false;
                    }
                }
                else
                {
                    result = await HttpClient.PostAsync(OptimalRoute.Handle, HeadData, BodyData);
                }
                return result;
            };
        }
    }
}