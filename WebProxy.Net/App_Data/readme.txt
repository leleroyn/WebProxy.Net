//˵��
// 1.1- ��������
// {
//    "Name": "��Ŀ����",
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
// 1.2- �����
// {
//    "Name": "��Ŀ����",
//    "Command": "Finance.FinanceList",
//    "Handle": "http://xcbankservice.ucsmy.com/p2p/Finance/FinanceListNew",
//    "Handles": {
//      "handle1":"http://service.test.com/ControllerName/ActionName1",
//      "handle2":"http://service.test.com/ControllerName/ActionName2",
//    }
// }
// 1.3- �����
// {
//    "Name": "��ҳ",
//    "Command": "Home.Index",
//    "Handles": {
//      "notice":"http://xcbankservice.ucsmy.com/p2p/Home/Notices",
//      "banner":"http://xcbankservice.ucsmy.com/p2p/Home/Banner",
//    }
// }
//
// 2- �ֶ�˵��
// Name:����
// Command:�������ƣ����
// Handle:�������URL,����΢����������ַ
// Handles:�������URL,����΢����������ַ  ��Handle��Handles�����ѡһ�������������д����Ĭ��ʹ��Handle��������
// Version:����汾�ţ�ѡ�
// System:����ϵͳ����,[None(��ͬ��ֵ�򲻴�ֵ),PC,Android,IOS]��ѡ�
// -- Version��System�ֶ�����Route��ɸѡ����·�ɵ�����
// CacheTime:����ʱ�䣬��λ�루ѡ�
// CacheCondition:����������ѡ�
// -- CacheTime��CacheCondition�����ж������Ƿ�ʹ�û���͹�������Key
