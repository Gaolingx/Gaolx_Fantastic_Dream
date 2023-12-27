using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//功能：登陆注册业务系统
public class LoginSys : SystemRoot
{
    public static LoginSys Instance = null;
    public LoginWnd loginWnd;
    public CreateWnd createWnd;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init LoginSys...");
    }
    /// <summary>
    /// 进入登录场景
    /// </summary>
    public void EnterLogin()
    {
        //异步加载登录场景
        //在加载的过程中动态显示加载进度
        resSvc.AsyncLoadScene(Constants.SceneLogin, () => {
            //加载完成以后再打开注册登录界面
            loginWnd.SetWndState();
            //播放登录场景音效
            audioSvc.PlayBGMusic(Constants.BGLogin);

        });
        
    }

    public void RspLogin(GameMsg msg)
    {
        GameRoot.AddTips("登录成功");
        GameRoot.Instance.SetPlayerData(msg.rspLogin);

        if (msg.rspLogin.playerData.name == "")
        {
            //打开角色创建界面
            createWnd.SetWndState();
        }
        else
        {
            //进入主城TODO
        }
        //关闭登录界面
        loginWnd.SetWndState(false);
    }
}
