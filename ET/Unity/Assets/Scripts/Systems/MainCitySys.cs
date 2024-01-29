//功能：主城业务系统
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCitySys : SystemRoot
{
    public static MainCitySys Instance = null;

    public MainCityWnd maincityWnd;
    public GameObject PlayerCameraRoot;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init MainCitySys...");
    }

    public void EnterMainCity()
    {
        //通过id获取主城配置后，加载场景
        MapCfg mapData = resSvc.GetMapCfgData(Constants.MainCityMapID);
        //加载主城场景
        resSvc.AsyncLoadScene(mapData.sceneName, () =>
        {
            PECommon.Log("Init MainCitySys...");

            // 加载游戏主角
            LoadPlayer(mapData);

            //打开主城场景UI
            maincityWnd.SetWndState();

            //播放主城背景音乐
            audioSvc.PlayBGMusic(Constants.BGMainCity);

            //TODO 设置人物展示相机

        });
    }

    private void LoadPlayer(MapCfg mapData)
    {
        //获取Prefab实例化的对象
        GameObject player = resSvc.LoadPrefab(PathDefine.AssissnCityPlayerPrefab, true);
        //初始化玩家位置
        player.transform.position = mapData.playerBornPos;
        player.transform.localEulerAngles = mapData.playerBornRote;
        player.transform.localScale = new Vector3(1.5f,1.5f,1.5f);

        //相机初始化
        //获取场景中PlayerCameraRoot对象的引用
        PlayerCameraRoot = GameObject.Find("PlayerCameraRoot");
        PlayerCameraRoot.transform.position = mapData.mainCamPos;
        PlayerCameraRoot.transform.localEulerAngles = mapData.mainCamRote;

        
    }
}
