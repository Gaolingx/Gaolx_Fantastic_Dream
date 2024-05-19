namespace DarkGod.Main
{
//���ܣ���¼ע�����
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LoginWnd : WindowRoot
{
    public InputField iptAcct;
    public InputField iptPass;
    public Button btnEnter;
    public Button btnNotice;
    public Toggle btnRemember;  //��ס����ѡ��

    private int boolToInt(bool val)
    {
        if (val)
            return 1;
        else
            return 0;
    }

    private bool intToBool(int val)
    {
        if (val != 0)
            return true;
        else
            return false;
    }

    protected override void InitWnd()
    {
        base.InitWnd();

        //��ȡ���ش洢���˺�����
        if (PlayerPrefs.HasKey("Acct") && PlayerPrefs.HasKey("Pass") && btnRemember.isOn == true)
        {
            iptAcct.text = PlayerPrefs.GetString("Acct");
            iptPass.text = PlayerPrefs.GetString("Pass");
            btnRemember.isOn = intToBool(PlayerPrefs.GetInt("rememberPass", 1));
        }
        else
        {
            iptAcct.text = "";
            iptPass.text = "";
            btnRemember.isOn = intToBool(PlayerPrefs.GetInt("rememberPass", 0));
        }
    }


    /// <summary>
    /// ���������Ϸ
    /// </summary>
    public void ClickEnterBtn()
    {
        audioSvc.PlayUIAudio(Constants.UILoginBtn);

        string _acct = iptAcct.text;
        string _pass = iptPass.text;
        if (_acct != "" && _pass != "")
        {
            //���±��ش洢���˺�����
            PlayerPrefs.SetString("Acct", _acct);
            PlayerPrefs.SetString("Pass", _pass);
            PlayerPrefs.SetInt("rememberPass", boolToInt(btnRemember.isOn));

            //����������Ϣ�������¼
            GameMsg msg = new GameMsg
            {
                cmd = (int)CMD.ReqLogin,
                reqLogin = new ReqLogin
                {
                    acct = _acct,
                    pass = _pass
                }
            };
            //����������񣬷��Ͱ����˺������������Ϣ
            netSvc.SendMsg(msg);
        }
        else
        {
            GameRoot.AddTips("�˺Ż�����Ϊ��");
        }
    }

    public void ClicKNoticeBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        GameRoot.AddTips("�������ڿ�����...");
    }
}

}