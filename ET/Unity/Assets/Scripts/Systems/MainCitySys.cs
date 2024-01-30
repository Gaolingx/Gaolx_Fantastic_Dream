//功能：主城业务系统
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

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
        //玩家初始化
        //获取Prefab实例化的对象
        GameObject player = resSvc.LoadPrefab(PathDefine.AssissnCityPlayerPrefab, true);
        //初始化玩家位置
        player.transform.position = mapData.playerBornPos;
        player.transform.localEulerAngles = mapData.playerBornRote;
        player.transform.localScale = new Vector3(1.0f,1.0f,1.0f);

        //相机初始化
        //首先要加载虚拟相机的预制件
        GameObject CM_player = resSvc.LoadPrefab(PathDefine.AssissnCityCharacterCameraPrefab, true);
        //设置实例化对象时候的位置、旋转
        Vector3 CM_player_Pos = mapData.mainCamPos;
        Quaternion CM_player_Rote = Quaternion.Euler(mapData.mainCamRote);
        //实例化对象
        GameObject CM_player_instance = Instantiate(CM_player,CM_player_Pos,CM_player_Rote);

        // 获取虚拟相机预制件上的CinemachineVirtualCamera组件  
        CinemachineVirtualCamera cinemachineVirtualCamera = CM_player_instance.GetComponent<CinemachineVirtualCamera>();  
        //至此，我们应该获取到了预制件上面的cinemachineVirtualCamera（但愿能获取到）组件
        
        //至此，我们终于可以对预制件上面获取到的cinemachineVirtualCamera组件进行操作了...>_<

        // 设置CinemachineVirtualCamera的跟随目标为标签为"PlayerCamRoot"的游戏对象的transform
        cinemachineVirtualCamera.Follow = GameObject.FindGameObjectWithTag("PlayerCamRoot").transform;



    }
}
