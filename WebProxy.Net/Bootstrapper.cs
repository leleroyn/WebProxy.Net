using Nancy;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using WebProxy.Common;
using WebProxy.Modules;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Ucsmy.Usp.Api;

namespace WebProxy
{

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public static string RootPath { get; set; }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            if (string.IsNullOrWhiteSpace(RootPath))
            {
                RootPath = RootPathProvider.GetRootPath();
            }

            Ucsmy.Usp.Api.ApiBaseService.Initialize();
        }      

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);        

            pipelines.OnError += (ctx, ex) =>
            {                
                LogHelper.Error("Route request error[Global]", string.Format("Route request error，Message:{0}", ex.Message), ex);             
                dynamic response = new ExpandoObject();
                response.Code = "500";
                response.ErrorMessage = ex.Message;
                return JsonConvert.SerializeObject(response);
            };
        }
    }
}