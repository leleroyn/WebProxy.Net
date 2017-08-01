using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using RestSharp;

namespace WebProxy.Common
{
    public static class HttpClient
    {
        public static async Task<string> PostAsync(Dictionary<string, string> handles, RequestHead head, Dictionary<string, object> body)
        {
            Dictionary<string, Task> requestDic = new Dictionary<string, Task>();
            foreach (var handle in handles)
            {
                var name = handle.Key;
                var url = handle.Value;

                RestRequest request = CreateRestRequest(head, body);
                var client = new RestClient(url)
                {
                    Proxy = null,
                    CookieContainer = null,
                    FollowRedirects = false,
                    Timeout = 60000
                };

                Task<IRestResponse> task = client.ExecuteTaskAsync(request);

                requestDic.Add(name, task);
            }

            var taskList = requestDic.Select(x => x.Value).ToArray();
            await Task.WhenAll(taskList);

            var responseDatas = requestDic.Select(x => new ResponseData() { Name = x.Key, Content = ((Task<IRestResponse>)x.Value).Result.Content });
            //单请求直接返回请求内容，多请求返回name-content的数组
            if (responseDatas.Count() == 1)
            {
                return JsonConvert.SerializeObject(responseDatas.First().Content);
            }

            return JsonConvert.SerializeObject(responseDatas);
        }


        public static async Task<string> PostAsync(string url, RequestHead head, Dictionary<string, object> body)
        {
            RestRequest request = CreateRestRequest(head, body);
            var client = new RestClient(url)
            {
                Proxy = null,
                CookieContainer = null,
                FollowRedirects = false,
                Timeout = 60000
            };

            var respones = await client.ExecuteTaskAsync(request);
            return respones.Content;
        }

        private static RestRequest CreateRestRequest(RequestHead head, Dictionary<string, object> body)
        {
            //-- Head
            dynamic headData = new ExpandoObject();
            headData.SerialNumber = head.SerialNumber;
            headData.Channel = head.Channel;
            headData.RequestHost = head.RequestHost;
            //channel: web,wap,app,cache
            headData.RequestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            headData.UserId = head.UserId;

            string headStr = JsonConvert.SerializeObject(headData);
            string postHead = EncodingHelper.Base64UrlEncode(Encoding.UTF8.GetBytes(headStr));

            //-- Body
            string bodyStr = JsonConvert.SerializeObject(body);
            string postBody = EncodingHelper.Base64UrlEncode(Encoding.UTF8.GetBytes(bodyStr));

            //-- Post Data
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("head", postHead);
            request.AddParameter("body", postBody);


            return request;
        }
    }
}