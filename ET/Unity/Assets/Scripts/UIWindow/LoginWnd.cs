using PEProtocol;
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

        string _acct = iptAcct.text;
        string _pass = iptPass.text;
        if(_acct != "" && _pass != "")
        {
            //���±��ش洢���˺�����
            PlayerPrefs.SetString("Acct", _acct);
            PlayerPrefs.SetString("Pass", _pass);

            //����������Ϣ�������¼

            GameMsg msg = new GameMsg
            {
                //ָ������״̬�루���������¼��
                cmd = (int)CMD.ReqLogin,
                reqLogin = new ReqLogin
                {
                    acct = _acct,  //�ұߵ�acctָ�����û�������ַ�������ߵ�acctָ����ReqLogin�ڵ�acct�ֶ�
                    pass = _pass
                }
            };
            netSvc.SendMsg(msg);

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
