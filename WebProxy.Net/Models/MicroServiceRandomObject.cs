using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProxy.Models
{
    public class MicroServiceRandomObject:RandomObject
    {
        /// <summary>
        /// 微服务提供地址
        /// </summary>
        public string ServiceUrl { get; set; }
    }
}