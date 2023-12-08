using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//功能：登录注册界面
public class LoginWnd : WindowRoot
{
    public InputField iptAcct;
    public InputField iptPass;
    public Button btnEnter;
    public Button btnNotice;
    public Toggle btnRemember;  //记住密码选项

    protected override void InitWnd()
    {
        base.InitWnd();

        //获取本地存储的账号密码
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

    //TODO 更新本地存储的账号密码

}
