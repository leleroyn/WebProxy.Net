 

1- ·������˵��

1.1- ��������
{
	"Name": "��Ŀ����",
	"Command": "Finance.FinanceList",
	"Handle": "Finance/FinanceListNew",
	"Version": "1.2.0",
	"System": "PC",
	"CacheTime": 30,
	"CacheCondition": {
		"PageIndex": "1,2,3",
		"ProjectType": "AnXiang"
     }��   
    "MicroService": "p2p"
}
1.2- �����
{
	"Name": "��Ŀ����",
	"Command": "Finance.FinanceList",
	"Handle": "p2p/Finance/FinanceListNew"��   
    "MicroService": "p2p"
}

2- �ֶ�˵��
Name:����
Command:�������ƣ����
Handle:�������URL,����΢����������ַ�������
Version:����汾�ţ�ѡ�
System:����ϵͳ����,[None(��ͬ��ֵ�򲻴�ֵ),PC,Android,IOS]��ѡ�
-- Version��System�ֶ�����Route��ɸѡ����·�ɵ�����
CacheTime:����ʱ�䣬��λ�루ѡ�
CacheCondition:����������ѡ�
-- CacheTime��CacheCondition�����ж������Ƿ�ʹ�û���͹�������Key
MicroService:hosts.json�ж����΢���������
