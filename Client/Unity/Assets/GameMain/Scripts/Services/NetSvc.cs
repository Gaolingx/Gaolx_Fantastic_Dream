//功能：网络服务
using System.Collections.Generic;
using PENet;
using PEProtocol;
using UnityEngine;
using HuHu;

namespace DarkGod.Main
{
    public class NetSvc : Singleton<NetSvc>
    {
        private static readonly string obj = "lock";
        PESocket<ClientSession, GameMsg> client = null;
        private Queue<GameMsg> msgQue = new Queue<GameMsg>();

        protected override void Awake()
        {
            base.Awake();
        }

        public void InitSvc()
        {
            client = new PESocket<ClientSession, GameMsg>();
            client.SetLog(true, (string msg, int lv) =>
            {
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
                MsgBox.MainInstance.ShowMessageBox("服务器未连接");
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

        //消息分发
        private void ProcessMsg(GameMsg msg)
        {
            if (msg.err != (int)ErrorCode.None)
            {
                //遇到错误弹出提示，返回，停止处理后面的业务逻辑
                switch ((ErrorCode)msg.err)
                {
                    case ErrorCode.ServerDataError:
                        PECommon.Log("服务器数据异常", PELogType.Error);
                        MsgBox.MainInstance.ShowMessageBox("客户端数据异常");
                        break;
                    case ErrorCode.UpdateDBError:
                        PECommon.Log("数据库更新异常", PELogType.Error);
                        MsgBox.MainInstance.ShowMessageBox("网络不稳定");
                        break;
                    case ErrorCode.ClientDataError:
                        PECommon.Log("客户端数据异常", PELogType.Error);
                        break;
                    case ErrorCode.AcctIsOnline:
                        MsgBox.MainInstance.ShowMessageBox("当前账号已经上线");
                        break;
                    case ErrorCode.WrongPass:
                        MsgBox.MainInstance.ShowMessageBox("密码错误");
                        break;
                    case ErrorCode.LackLevel:
                        MsgBox.MainInstance.ShowMessageBox("角色等级不够");
                        break;
                    case ErrorCode.LackCoin:
                        MsgBox.MainInstance.ShowMessageBox("金币数量不够");
                        break;
                    case ErrorCode.LackCrystal:
                        MsgBox.MainInstance.ShowMessageBox("水晶数量不够");
                        break;
                    case ErrorCode.LackDiamond:
                        MsgBox.MainInstance.ShowMessageBox("钻石数量不够");
                        break;
                    case ErrorCode.LackPower:
                        MsgBox.MainInstance.ShowMessageBox("体力值不足");
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
                case CMD.RspFBFightEnd:
                    BattleSys.Instance.RspFightEnd(msg);
                    break;
            }
        }
    }
}