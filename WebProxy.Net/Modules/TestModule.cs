using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Text;
using System.Web;
using WebProxy.Common;
using Nancy;
using Newtonsoft.Json;
using RestSharp;
using WebProxy.Models;

namespace WebProxy.Modules
{
    public class TestModule : NancyModule
    {
        public TestModule()
        {
            Get["/Test"] = _ =>
            {
                return View["Test"];
            };

            Post["/Test"] = _ =>
            {
                //- Get Data
                RequestHead head = new RequestHead();
                head.Command = Request.Form["command"];
                head.Version = Request.Form["version"];
                head.System = Request.Form["system"];
                head.Channel = Request.Form["channel"];
                head.UserId = Request.Form["userid"];
                head.UseCache = Request.Form["usecache"];
                head.MultiRequestType = Request.Form["multirequesttype"];

                head.SerialNumber = Guid.NewGuid().ToString();
                head.RequestHost = "127.0.0.1";
                head.RequestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string headData = JsonConvert.SerializeObject(head);
                headData = EncodingHelper.Base64UrlEncode(Encoding.UTF8.GetBytes(headData));

                string bodyData = Request.Form["body"];
                bodyData = EncodingHelper.Base64UrlEncode(Encoding.UTF8.GetBytes(bodyData));
                string encryptBody = EncryptHelper.DESEncrypt(bodyData, SettingsHelper.GetDesKey(head.Channel));

                string url = Request.Url.SiteBase + "/Api";

                //- Post
                RestClient client = new RestClient(url);
                client.Proxy = null;
                client.Timeout = 60000;
                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("head", headData);
                request.AddParameter("body", encryptBody);
                string result = client.Execute(request).Content;

                //if (head.Command.Contains(Settings.MultiCommandSplitChar))
                //{
                //    string[] cmds = head.Command.Split(Settings.MultiCommandSplitChar);
                //    Dictionary<string, ResponseMsg> responseDic = new Dictionary<string, ResponseMsg>();
                //    foreach (var response in JsonConvert.DeserializeObject<Dictionary<string, string>>(result))
                //    {
                //        var val = JsonConvert.DeserializeObject<ResponseMsg>(response.Value);
                //        responseDic.Add(response.Key, val);
                //    }
                //}

                return result;
            };
        }

        //public class ResponseMsg
        //{
        //    public string respCode { get; set; }
        //    public string SN { get; set; }
        //    public string respMsg { get; set; }
        //    public Dictionary<string, object> respData { get; set; }
        //}
    }
}