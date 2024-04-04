//功能：游戏启动入口，初始化各个业务系统
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        PECommon.Log("Game Start...");

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
        NetSvc net = GetComponent<NetSvc>();
        net.InitSvc();

        ResSvc res = GetComponent<ResSvc>();
        res.InitSvc();
        AudioSvc audio = GetComponent<AudioSvc>();
        audio.InitSvc();
        NpcCfg npcCfg = GetComponent<NpcCfg>();
        npcCfg.InitCfg();
        TimerSvc timer = GetComponent<TimerSvc>();
        timer.InitSvc();


        //业务系统初始化
        LoginSys loginSys = GetComponent<LoginSys>();
        loginSys.InitSys();
        MainCitySys maincitySys = GetComponent<MainCitySys>();
        maincitySys.InitSys();
        FubenSys fubenSys = GetComponent<FubenSys>();
        fubenSys.InitSys();
        BattleSys battleSys = GetComponent<BattleSys>();
        battleSys.InitSys();

        //进入登录场景并加载相应UI
        loginSys.EnterLogin();

    }

    public GameObject GetEventSystem()
    {
        GameObject eventSystem = null;
        eventSystem = GameObject.Find(Constants.EventSystemGOName);
        return eventSystem;
    }

    public static void AddTips(string tips)
    {
        Instance.dynamicWnd.AddTips(tips);
    }

    private PlayerData _playerData = null;
    public PlayerData PlayerData { get { return _playerData; } }
    public void SetPlayerData(RspLogin data)
    {
        _playerData = data.playerData;
    }

    public void SetPlayerName(string name)
    {
        PlayerData.name = name;
    }

    public void SetPlayerDataByGuide(RspGuide data)
    {
        PlayerData.coin = data.coin;
        PlayerData.lv = data.lv;
        PlayerData.exp = data.exp;
        PlayerData.guideid = data.guideid;
    }

    public void SetPlayerDataByStrong(RspStrong data)
    {
        PlayerData.coin = data.coin;
        PlayerData.crystal = data.crystal;
        PlayerData.hp = data.hp;
        PlayerData.ad = data.ad;
        PlayerData.ap = data.ap;
        PlayerData.addef = data.addef;
        PlayerData.apdef = data.apdef;

        PlayerData.strongArr = data.strongArr;
    }

    public void SetPlayerDataByBuy(RspBuy data)
    {
        PlayerData.diamond = data.diamond;
        PlayerData.coin = data.coin;
        PlayerData.power = data.power;
    }

    public void SetPlayerDataByPower(PshPower data)
    {
        PlayerData.power = data.power;
    }

    public Transform SetGameObjectTrans(GameObject GO, Vector3 GameObjectPos, Vector3 GameObjectRota, Vector3 GameObjectScal)
    {
        GO.transform.position = GameObjectPos;
        GO.transform.localEulerAngles = GameObjectRota;
        GO.transform.localScale = GameObjectScal;

        Transform GOTrans = GO.transform;
        return GOTrans;
    }

    public void SetPlayerDataByTask(RspTakeTaskReward data)
    {
        PlayerData.coin = data.coin;
        PlayerData.lv = data.lv;
        PlayerData.exp = data.exp;
        PlayerData.taskArr = data.taskArr;
    }

    public void SetPlayerDataByTaskPsh(PshTaskPrgs data)
    {
        PlayerData.taskArr = data.taskArr;
    }

    public void SetPlayerDataByFBStart(RspFBFight data)
    {
        PlayerData.power = data.power;
    }

}
