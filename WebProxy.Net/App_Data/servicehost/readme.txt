
host配置说明

完整实例：
{
    "Name": "p2p",
    "Hosts": [
      {
        "ServiceUrl": "http://xcbankservice.ucsmy.com/p2p1",
        "Weight": 20
      },
      {
        "ServiceUrl": "http://xcbankservice.ucsmy.com/p2p2",
        "Weight": 80
      }
    ]
}

最简实例：
{
    "Name": "p2p",
    "Hosts": [
      {
        "ServiceUrl": "http://xcbankservice.ucsmy.com/p2p",
        "Weight": 100
      }
    ]
}

Name：host名称，关联路由配置的MicroService
Hosts：微服务地址，如存在负载，则配置多项，系统通过权重自动进行负载
	ServiceUrl:微服务路径
	Weight:负载权重，建议使用100进行拆分赋值权重，如（20,80）。

