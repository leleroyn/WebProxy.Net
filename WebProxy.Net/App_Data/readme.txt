//说明
// 1.1- 完整范例
// {
//    "Name": "项目详情",
//    "Command": "Finance.FinanceList",
//    "Handle": "http://xcbankservice.ucsmy.com/p2p/Finance/FinanceListNew",
//    "Version": "1.2.0",
//    "System": "PC",
//    "CacheTime": 30,
//    "CacheCondition": {
//      "PageIndex": "1,2,3",
//      "ProjectType": "AnXiang"
//    }
// }
// 1.2- 最简范例
// {
//    "Name": "项目详情",
//    "Command": "Finance.FinanceList",
//    "Handle": "http://xcbankservice.ucsmy.com/p2p/Finance/FinanceListNew",
//    "Handles": {
//      "handle1":"http://service.test.com/ControllerName/ActionName1",
//      "handle2":"http://service.test.com/ControllerName/ActionName2",
//    }
// }
// 1.3- 最简范例
// {
//    "Name": "首页",
//    "Command": "Home.Index",
//    "Handles": {
//      "notice":"http://xcbankservice.ucsmy.com/p2p/Home/Notices",
//      "banner":"http://xcbankservice.ucsmy.com/p2p/Home/Banner",
//    }
// }
//
// 2- 字段说明
// Name:名称
// Command:命令名称（必填）
// Handle:单命令处理URL,对于微服务的请求地址
// Handles:多命令处理URL,对于微服务的请求地址  （Handle和Handles必须二选一，如果两个都填写，则默认使用Handle单请求处理）
// Version:请求版本号（选填）
// System:请求系统类型,[None(等同空值或不传值),PC,Android,IOS]（选填）
// -- Version和System字段用于Route端筛选最优路由的条件
// CacheTime:缓存时间，单位秒（选填）
// CacheCondition:缓存条件（选填）
// -- CacheTime和CacheCondition用于判断请求是否使用缓存和构建缓存Key
