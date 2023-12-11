using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//功能：登陆注册业务系统
public class LoginSys : SystemRoot
{
    public static LoginSys Instance = null;
    public LoginWnd loginWnd;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        Debug.Log("Init LoginSys...");
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
            GameRoot.AddTips("Load Done");
            GameRoot.AddTips("Load Done 2");
        });
        
    }

}
