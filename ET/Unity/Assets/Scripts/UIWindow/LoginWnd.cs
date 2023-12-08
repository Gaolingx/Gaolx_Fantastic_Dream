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

    //TODO ���±��ش洢���˺�����

}
