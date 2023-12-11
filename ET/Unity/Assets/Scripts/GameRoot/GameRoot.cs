using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//功能：游戏启动入口，初始化各个业务系统
public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance = null;

    public LoadingWnd loadingWnd;
    public DynamicWnd dynamicWnd;
    private void Start()
    {
        Instance = this;
        //我们不希望GameRoot及其子物体在切换场景时被销毁
        DontDestroyOnLoad(this);
        Debug.Log("Game Start...");

        CleanUIRoot();

        Init();
    }

    private void CleanUIRoot()
    {
        Transform canvas = transform.Find("Canvas");
        for (int i = 0; i < canvas.childCount; i++)
        {
            canvas.GetChild(i).gameObject.SetActive(false);
        }

        dynamicWnd.SetWndState();
    }

    //初始化各个系统和服务模块
    private void Init()
    {
        //注：需要先初始化服务模块
        //服务模块初始化
        ResSvc res = GetComponent<ResSvc>();
        res.InitSvc();
        AudioSvc audio = GetComponent<AudioSvc>();
        audio.InitSvc();


        //业务系统初始化
        LoginSys login = GetComponent<LoginSys>();
        login.InitSys();


        //进入登录场景并加载相应UI
        login.EnterLogin();

    }

    public static void AddTips(string tips)
    {
        Instance.dynamicWnd.AddTips(tips);
    }
}
