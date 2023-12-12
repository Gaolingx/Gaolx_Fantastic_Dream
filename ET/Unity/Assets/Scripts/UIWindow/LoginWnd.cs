using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//���ܣ���¼ע�����
public class LoginWnd : WindowRoot
{
    public InputField iptAcct;
    public InputField iptPass;
    public Button btnEnter;
    public Button btnNotice;
    public Toggle btnRemember;  //��ס����ѡ��

    protected override void InitWnd()
    {
        base.InitWnd();

        //��ȡ���ش洢���˺�����
        if(PlayerPrefs.HasKey("Acct") && PlayerPrefs.HasKey("Pass") && btnRemember.isOn == true)
        {
            iptAcct.text = PlayerPrefs.GetString("Acct");
            iptPass.text = PlayerPrefs.GetString("Pass");
        }
        else
        {
            iptAcct.text = "";
            iptPass.text = "";
        }
    }


    /// <summary>
    /// ���������Ϸ
    /// </summary>
    public void ClickEnterBtn()
    {
        audioSvc.PlayUIAudio(Constants.UILoginBtn);

        string acct = iptAcct.text;
        string pass = iptPass.text;
        if(acct != "" && pass != "")
        {
            //���±��ش洢���˺�����
            PlayerPrefs.SetString("Acct", acct);
            PlayerPrefs.SetString("Pass", pass);

            //TODO ����������Ϣ�������¼

            //TO Remove
            LoginSys.Instance.RspLogin();
        }
        else
        {
            GameRoot.AddTips("�˺Ż����벻��Ϊ��Ŷ��");
        }
    }

    public void ClickNoticeBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        GameRoot.AddTips("�������ڿ�����...");
    }
}
