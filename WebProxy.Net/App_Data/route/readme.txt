 

·������˵��

1.1- ��������
{
	"Name": "��Ŀ����",
	"Command": "Finance.FinanceList",
	"Handle": "http:xcbankservice.ucsmy.com/p2p/Finance/FinanceListNew",
	"Version": "1.2.0",
	"System": "PC",
	"CacheTime": 30,
	"CacheCondition": {
		"PageIndex": "1,2,3",
		"ProjectType": "AnXiang"
}
}
1.2- �����
{
	"Name": "��Ŀ����",
	"Command": "Finance.FinanceList",
	"Handle": "http:xcbankservice.ucsmy.com/p2p/Finance/FinanceListNew"
}
�� - ʹ��ռλ��
{
	"Name": "��Ŀ����",
	"Command": "Finance.FinanceList",
	"Handle": "${p2p}/Finance/FinanceListNew"
}

2- �ֶ�˵��
Name:����
Command:�������ƣ����
Handle:�������URL,����΢����������ַ�������handle��ʹ��ռλ��${hostname}����http·������ʹ��ռλ����Ĭ��ʹ��host���ý��и��ؾ���
Version:����汾�ţ�ѡ�
System:����ϵͳ����,[None(��ͬ��ֵ�򲻴�ֵ),PC,Android,IOS]��ѡ�
-- Version��System�ֶ�����Route��ɸѡ����·�ɵ�����
CacheTime:����ʱ�䣬��λ�루ѡ�
CacheCondition:����������ѡ�
-- CacheTime��CacheCondition�����ж������Ƿ�ʹ�û���͹�������Key
