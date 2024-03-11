//功能：游戏启动入口，初始化各个业务系统
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
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


        //业务系统初始化
        LoginSys login = GetComponent<LoginSys>();
        login.InitSys();
        MainCitySys maincitySys = GetComponent<MainCitySys>();
        maincitySys.InitSys();

        //进入登录场景并加载相应UI
        login.EnterLogin();

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
        PlayerData.diamond = data.dimond;
        PlayerData.coin = data.coin;
        PlayerData.power = data.power;
    }
}
