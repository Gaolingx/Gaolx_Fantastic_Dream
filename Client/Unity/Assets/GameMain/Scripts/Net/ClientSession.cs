//功能：客户端网络会话
using PENet;
using PEProtocol;


namespace DarkGod.Main
{
    public class ClientSession : PESession<GameMsg>
    {
        protected override void OnConnected()
        {
            MsgBox.MainInstance.ShowMessageBox("连接服务器成功");
            PECommon.Log("Connect To Server Succ");
        }

        protected override void OnReciveMsg(GameMsg msg)
        {
            PECommon.Log("RcvPack CMD:" + ((CMD)msg.cmd).ToString());
            NetSvc.MainInstance.AddNetPkg(msg);
        }

        protected override void OnDisConnected()
        {
            MsgBox.MainInstance.ShowMessageBox("服务器断开连接");
            PECommon.Log("DisConnect To Server");
        }
    }
}
