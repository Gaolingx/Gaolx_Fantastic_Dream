//功能：主城业务系统
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCitySys : SystemRoot
{
    public static MainCitySys Instance = null;

    public MainCityWnd maincityWnd;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init MainCitySys...");
    }

    public void EnterMainCity()
    {
        //加载主城场景
        resSvc.AsyncLoadScene(Constants.SceneMainCity, () =>
        {
            PECommon.Log("Init MainCitySys...");

            //TODO 加载游戏主角

            //打开主城场景UI
            maincityWnd.SetWndState();

            //播放主城背景音乐
            audioSvc.PlayBGMusic(Constants.BGMainCity);

            //TODO 设置人物展示相机

        });
    }
}
