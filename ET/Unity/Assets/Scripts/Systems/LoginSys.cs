using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//功能：登陆注册业务系统
public class LoginSys : MonoBehaviour
{
    public void InitSys()
    {
        Debug.Log("Init LoginSys...");
    }
    /// <summary>
    /// 进入登录场景
    /// </summary>
    public void EnterLogin()
    {
        GameRoot.Instance.loadingWnd.gameObject.SetActive(true);
        GameRoot.Instance.loadingWnd.InitWnd();

        //异步加载登录场景
        ResSvc.Instance.AsyncLoadScene(Constants.SceneLogin);
        //在加载的过程中动态显示加载进度
        //加载完成以后再打开注册登录界面
    }


}
