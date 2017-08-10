using Nancy;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using WebProxy.Common;
using WebProxy.Modules;
using Nancy.Bootstrapper;
using Nancy.Json;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Ucsmy.Usp.Api;

namespace WebProxy
{

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            if (string.IsNullOrWhiteSpace(Settings.RootPath))
            {
                Settings.RootPath = RootPathProvider.GetRootPath();
            }

            // 默认情况下，nancy在tojson时将对json key进行大小写装换，保存大小写需设置RetainCasing为true
            // Serialize: NotificationId->notificationId
            // Deserialize: notificationId->NotificationId
            JsonSettings.RetainCasing = true;

            //初始化日志
            ApiBaseService.Initialize();
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