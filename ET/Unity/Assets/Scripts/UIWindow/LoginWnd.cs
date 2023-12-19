using PEProtocol;
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


    /// <summary>
    /// 点击进入游戏
    /// </summary>
    public void ClickEnterBtn()
    {
        audioSvc.PlayUIAudio(Constants.UILoginBtn);

        string _acct = iptAcct.text;
        string _pass = iptPass.text;
        if(_acct != "" && _pass != "")
        {
            //更新本地存储的账号密码
            PlayerPrefs.SetString("Acct", _acct);
            PlayerPrefs.SetString("Pass", _pass);

            //发送网络消息，请求登录

            GameMsg msg = new GameMsg
            {
                //指定请求状态码（用于请求登录）
                cmd = (int)CMD.ReqLogin,
                reqLogin = new ReqLogin
                {
                    acct = _acct,  //右边的acct指的是用户输入的字符串，左边的acct指的是ReqLogin内的acct字段
                    pass = _pass
                }
            };
            netSvc.SendMsg(msg);

        }
        else
        {
            GameRoot.AddTips("账号或密码不能为空哦！");
        }
    }

    public void ClickNoticeBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        GameRoot.AddTips("功能正在开发中...");
    }
}
