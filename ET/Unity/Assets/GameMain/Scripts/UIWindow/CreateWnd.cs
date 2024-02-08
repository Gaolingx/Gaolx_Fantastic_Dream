//功能：角色创建界面
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CreateWnd : WindowRoot
{
    public InputField iptName;

    protected override void InitWnd()
    {
        base.InitWnd();


        //显示一个随机名字
        iptName.text = resSvc.GetRDNameData(false);
    }

    public void ClickRandBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        string rdName = resSvc.GetRDNameData(false);
        iptName.text = rdName;
    }

    public void ClickEnterBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        if (iptName.text != "")
        {
            //发送名字数据到服务器，登录主城
            GameMsg msg = new GameMsg
            {
                cmd = (int)CMD.ReqRename,
                reqRename = new ReqRename
                {
                    name = iptName.text
                }
            };
            netSvc.SendMsg(msg);
        }
        else
        {
            GameRoot.AddTips("当前名字不符合规范");
        }
    }
}
