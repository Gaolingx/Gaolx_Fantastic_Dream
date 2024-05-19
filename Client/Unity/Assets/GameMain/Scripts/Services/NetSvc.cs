namespace DarkGod.Main
{
//���ܣ��������
using System.Collections.Generic;
using PENet;
using PEProtocol;
using UnityEngine;


public class NetSvc : MonoBehaviour
{
    public static NetSvc Instance = null;

    private static readonly string obj = "lock";
    PESocket<ClientSession, GameMsg> client = null;
    private Queue<GameMsg> msgQue = new Queue<GameMsg>();


    public void InitSvc()
    {
        Instance = this;

        client = new PESocket<ClientSession, GameMsg>();
        client.SetLog(true, (string msg, int lv) => {
            switch (lv)
            {
                case 0:
                    msg = "Log:" + msg;
                    Debug.Log(msg);
                    break;
                case 1:
                    msg = "Warn:" + msg;
                    Debug.LogWarning(msg);
                    break;
                case 2:
                    msg = "Error:" + msg;
                    Debug.LogError(msg);
                    break;
                case 3:
                    msg = "Info:" + msg;
                    Debug.Log(msg);
                    break;
            }
        });
        client.StartAsClient(SrvCfg.srvIP, SrvCfg.srvPort);
        PECommon.Log("Init NetSvc...");
    }

    public void SendMsg(GameMsg msg)
    {
        if (client.session != null)
        {
            client.session.SendMsg(msg);
        }
        else
        {
            GameRoot.AddTips("������δ����");
            InitSvc();
        }
    }

    public void AddNetPkg(GameMsg msg)
    {
        lock (obj)
        {
            msgQue.Enqueue(msg);
        }
    }

    private void Update()
    {
        if (msgQue.Count > 0)
        {
            lock (obj)
            {
                GameMsg msg = msgQue.Dequeue();
                ProcessMsg(msg);
            }
        }
    }

    //��Ϣ�ַ�
    private void ProcessMsg(GameMsg msg)
    {
        if (msg.err != (int)ErrorCode.None)
        {
            //�������󵯳���ʾ�����أ�ֹͣ��������ҵ���߼�
            switch ((ErrorCode)msg.err)
            {
                case ErrorCode.ServerDataError:
                    PECommon.Log("�����������쳣", PELogType.Error);
                    GameRoot.AddTips("�ͻ��������쳣");
                    break;
                case ErrorCode.UpdateDBError:
                    PECommon.Log("���ݿ�����쳣", PELogType.Error);
                    GameRoot.AddTips("���粻�ȶ�");
                    break;
                case ErrorCode.ClientDataError:
                    PECommon.Log("�ͻ��������쳣", PELogType.Error);
                    break;
                case ErrorCode.AcctIsOnline:
                    GameRoot.AddTips("��ǰ�˺��Ѿ�����");
                    break;
                case ErrorCode.WrongPass:
                    GameRoot.AddTips("�������");
                    break;
                case ErrorCode.LackLevel:
                    GameRoot.AddTips("��ɫ�ȼ�����");
                    break;
                case ErrorCode.LackCoin:
                    GameRoot.AddTips("�����������");
                    break;
                case ErrorCode.LackCrystal:
                    GameRoot.AddTips("ˮ����������");
                    break;
                case ErrorCode.LackDiamond:
                    GameRoot.AddTips("��ʯ��������");
                    break;
                case ErrorCode.LackPower:
                    GameRoot.AddTips("����ֵ����");
                    break;
            }
            return;
        }

        switch ((CMD)msg.cmd)
        {
            case CMD.RspLogin:
                LoginSys.Instance.RspLogin(msg);
                break;
            case CMD.RspRename:
                LoginSys.Instance.RspRename(msg);
                break;
            case CMD.RspGuide:
                MainCitySys.Instance.RspGuide(msg);
                break;
            case CMD.RspStrong:
                MainCitySys.Instance.RspStrong(msg);
                break;
            case CMD.PshChat:
                MainCitySys.Instance.PshChat(msg);
                break;
            case CMD.RspBuy:
                MainCitySys.Instance.RspBuy(msg);
                break;
            case CMD.PshPower:
                MainCitySys.Instance.PshPower(msg);
                break;
            case CMD.RspTakeTaskReward:
                MainCitySys.Instance.RspTakeTaskReward(msg);
                break;
            case CMD.PshTaskPrgs:
                MainCitySys.Instance.PshTaskPrgs(msg);
                break;
            case CMD.RspFBFight:
                FubenSys.Instance.RspFBFight(msg);
                break;
        }
    }
}
}