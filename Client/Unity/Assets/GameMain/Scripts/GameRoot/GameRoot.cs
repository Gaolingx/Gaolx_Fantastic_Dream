namespace DarkGod.Main
{
//���ܣ���Ϸ�����ڣ���ʼ������ҵ��ϵͳ
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
        //���ǲ�ϣ��GameRoot�������������л�����ʱ������
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
    }

    //��ʼ������ϵͳ�ͷ���ģ��
    private void Init()
    {
        //ע����Ҫ�ȳ�ʼ������ģ��
        //����ģ���ʼ��
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


        //ҵ��ϵͳ��ʼ��
        LoginSys loginSys = GetComponent<LoginSys>();
        loginSys.InitSys();
        MainCitySys maincitySys = GetComponent<MainCitySys>();
        maincitySys.InitSys();
        FubenSys fubenSys = GetComponent<FubenSys>();
        fubenSys.InitSys();
        BattleSys battleSys = GetComponent<BattleSys>();
        battleSys.InitSys();

        dynamicWnd.SetWndState();
        //�����¼������������ӦUI
        loginSys.EnterLogin();

    }

    private Dictionary<string, GameObject> goDic = new Dictionary<string, GameObject>();
    public GameObject GetEventSystemObject(string path, bool iscache = true)
    {
        GameObject eventSystem = null;
        if (!goDic.TryGetValue(path, out eventSystem))
        {
            eventSystem = GameObject.Find(path);
            if(iscache)
            {
                goDic.Add(path, eventSystem);
            }
        }
        return eventSystem;
    }

    public void PauseGameUI(bool state = true)
    {
        GetEventSystemObject(Constants.EventSystemGOName).GetComponent<UIController>().isPause = state;
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

    public Transform SetGameObjectTrans(GameObject GO, Vector3 GameObjectPos, Vector3 GameObjectRota, Vector3 GameObjectScal, bool isLocalPos = false)
    {
        if (isLocalPos)
        {
            GO.transform.localPosition = GameObjectPos;
        }
        else
        {
            GO.transform.position = GameObjectPos;
        }

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

    private EntityPlayer entityPlayer = null;
    public void SetCurrentPlayer(EntityPlayer player)
    {
        entityPlayer = player;
    }
    public EntityPlayer GetCurrentPlayer()
    {
        return entityPlayer;
    }

}

}