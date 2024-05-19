namespace DarkGod.Main
{
//���ܣ���ɫ��������
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


        //��ʾһ���������
        iptName.text = resSvc.GetRDNameCfg(false);
    }

    public void ClickRandBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        string rdName = resSvc.GetRDNameCfg(false);
        iptName.text = rdName;
    }

    public void ClickEnterBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        if (iptName.text != "")
        {
            //�����������ݵ�����������¼����
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
            GameRoot.AddTips("��ǰ���ֲ����Ϲ淶");
        }
    }
}

}