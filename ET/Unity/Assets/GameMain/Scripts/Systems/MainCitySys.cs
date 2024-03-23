//功能：主城业务系统
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using PEProtocol;
using UnityEngine.InputSystem.XR;

public class MainCitySys : SystemRoot
{
    public static MainCitySys Instance = null;

    public MainCityWnd maincityWnd;
    public InfoWnd infoWnd;
    public GuideWnd guideWnd;
    public StrongWnd strongWnd;
    public SettingsWnd settingsWnd;
    public ChatWnd chatWnd;
    public BuyWnd buyWnd;
    public TaskWnd taskWnd;
    public FpsWnd fpsWnd;
    public UIController uiController;

    public GameObject PlayerCameraRoot;
    private GameObject Scene_player;
    private PlayerController playerCtrl;
    private Transform charCamTrans;
    private AutoGuideCfg curtTaskData;
    private Transform[] npcPosTrans;
    private NavMeshAgent nav;
    private StarterAssetsInputs playerInput;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init MainCitySys...");
    }

    public void EnterMainCity()
    {
        //通过id获取主城配置后，加载场景
        MapCfg mapData = resSvc.GetMapCfgCfg(Constants.MainCityMapID);
        //加载主城场景
        resSvc.AsyncLoadScene(mapData.sceneName, () =>
        {
            PECommon.Log("Init MainCitySys...");

            // 加载游戏主角
            LoadPlayer(mapData);

            // 加载NPC
            LoadNpcPrefab();

            //打开主城场景UI
            maincityWnd.SetWndState();

            // 初始化摇杆插件
            InitGamepad();

            //配置角色声音源
            AudioSvc.Instance.GetCharacterAudioSourceComponent();

            //播放主城背景音乐
            audioSvc.PlayBGMusic(Constants.BGMainCity);

            //获取主城NPCs的Transform
            GetMapNpcTransform();

            //设置人物展示相机
            if (charCamTrans != null)
            {
                charCamTrans.gameObject.SetActive(false);
            }

            //加载性能计数器
            fpsWnd.InitWnd();

        });

    }

    private void LoadPlayer(MapCfg mapData)
    {
        //玩家初始化
        //获取Prefab实例化的对象
        GameObject player = resSvc.LoadPrefab(PathDefine.AssissnCityPlayerPrefab, true);
        if (player != null)
        {
            Debug.Log(PathDefine.AssissnCityPlayerPrefab + " 预制件加载成功！");
            //初始化玩家位置
            GameRoot.Instance.SetGameObjectTrans(player, mapData.playerBornPos, mapData.playerBornRote, new Vector3(1.0f, 1.0f, 1.0f));

            //原方案
            //相机初始化
            /*
            Camera.main.transform.position = mapData.mainCamPos;
            Camera.main.transform.localEulerAngles = mapData.mainCamRote;

            playerCtrl = player.GetComponent<PlayerController>();
            playerCtrl.Init();
            */

            //获取player导航组件
            nav = player.GetComponent<NavMeshAgent>();

            player.GetComponent<ThirdPersonController>().MoveSpeed = Constants.PlayerMoveSpeed;
            player.GetComponent<ThirdPersonController>().SprintSpeed = Constants.PlayerSprintSpeed;

            playerInput = player.GetComponent<StarterAssetsInputs>();

            Scene_player = GameObject.FindGameObjectWithTag(Constants.CharPlayerWithTag);
        }
        else
        {
            Debug.LogError(PathDefine.AssissnCityPlayerPrefab + " 预制件加载失败！");
        }

        //相机初始化
        //首先要加载虚拟相机的预制件
        GameObject CM_player = resSvc.LoadPrefab(PathDefine.AssissnCityCharacterCameraPrefab, true);
        if (CM_player != null)
        {
            Debug.Log(PathDefine.AssissnCityCharacterCameraPrefab + " 预制件加载成功！");
            //设置实例化对象时候的位置、旋转
            Vector3 CM_player_Pos = mapData.mainCamPos;
            Vector3 CM_player_Rote = mapData.mainCamRote;
            GameRoot.Instance.SetGameObjectTrans(CM_player, CM_player_Pos, CM_player_Rote, Vector3.one);

            // 获取虚拟相机预制件上的CinemachineVirtualCamera组件  
            CinemachineVirtualCamera cinemachineVirtualCamera = CM_player.GetComponent<CinemachineVirtualCamera>();
            //至此，我们应该获取到了预制件上面的cinemachineVirtualCamera（但愿能获取到）组件

            //至此，我们终于可以对预制件上面获取到的cinemachineVirtualCamera组件进行操作了...>_<

            // 设置CinemachineVirtualCamera的跟随目标为标签为"PlayerCamRoot"的游戏对象的transform
            cinemachineVirtualCamera.Follow = GameObject.FindGameObjectWithTag(Constants.CinemachineVirtualCameraFollowGameObjectWithTag).transform;
            //通过读取配置表设置CinemachineVirtualCamera相裁剪平面
            cinemachineVirtualCamera.m_Lens.FarClipPlane = Constants.CinemachineVirtualCameraFarClipPlane;
            cinemachineVirtualCamera.m_Lens.NearClipPlane = Constants.CinemachineVirtualCameraNearClipPlane;
        }
        else
        {
            Debug.LogError(PathDefine.AssissnCityCharacterCameraPrefab + " 预制件加载失败！");
        }

    }

    private void LoadNpcPrefab()
    {
        NpcCfg.Instance.LoadMapNpc(0, PathDefine.AssissnCityNPC0Prefab);
        NpcCfg.Instance.LoadMapNpc(1, PathDefine.AssissnCityNPC1Prefab);
        NpcCfg.Instance.LoadMapNpc(2, PathDefine.AssissnCityNPC2Prefab);
        NpcCfg.Instance.LoadMapNpc(3, PathDefine.AssissnCityNPC3Prefab);

    }

    private void InitGamepad()
    {
        GameObject GamePad = GameObject.Find(Constants.GamepadBind_StarterAssetsInputs_Joysticks);
        UICanvasControllerInput uICanvasControllerInput = GamePad.GetComponent<UICanvasControllerInput>();
        StarterAssetsInputs StarterAssetsInputs_player = Scene_player.GetComponent<StarterAssetsInputs>();

        uICanvasControllerInput.starterAssetsInputs = StarterAssetsInputs_player;
    }


    //原方案
    public void SetMoveDir(Vector2 dir)
    {
        StopNavTask();

        //设置动画
        if (dir == Vector2.zero)
        {
            playerCtrl.SetBlend(Constants.BlendIdle);
        }
        else
        {
            playerCtrl.SetBlend(Constants.BlendWalk);
        }
        //设置方向
        playerCtrl.Dir = dir;
    }

    #region Task Wnd
    public void OpenTaskRewardWnd()
    {
        taskWnd.SetWndState();
    }
    public void RspTakeTaskReward(GameMsg msg)
    {
        RspTakeTaskReward data = msg.rspTakeTaskReward;
        GameRoot.Instance.SetPlayerDataByTask(data);

        taskWnd.RefreshUI();
        maincityWnd.RefreshUI();
    }
    public void PshTaskPrgs(GameMsg msg)
    {
        PshTaskPrgs data = msg.pshTaskPrgs;
        GameRoot.Instance.SetPlayerDataByTaskPsh(data);

        if(taskWnd.GetWndState())
        {
            taskWnd.RefreshUI();
        }
    }
    #endregion

    #region Buy Wnd
    public void OpenBuyWnd(int type)
    {
        buyWnd.SetBuyType(type);
        buyWnd.SetWndState();
    }
    public void RspBuy(GameMsg msg)
    {
        RspBuy rspBuydata = msg.rspBuy;
        //更新玩家数据到GameRoot中
        GameRoot.Instance.SetPlayerDataByBuy(rspBuydata);
        GameRoot.AddTips("购买成功");

        //更新主城界面
        maincityWnd.RefreshUI();
        //关闭购买窗口
        buyWnd.SetWndState(false);

        if(msg.pshTaskPrgs !=  null)
        {
            GameRoot.Instance.SetPlayerDataByTaskPsh(msg.pshTaskPrgs);
            if (taskWnd.GetWndState())
            {
                taskWnd.RefreshUI();
            }
        }
    }
    #endregion

    #region Power Handle
    public void PshPower(GameMsg msg)
    {
        PshPower data = msg.pshPower;
        GameRoot.Instance.SetPlayerDataByPower(data);
        if(maincityWnd.GetWndState())
        {
            maincityWnd.RefreshUI();
        }
        
    }
    #endregion

    #region Chat Wnd
    public void OpenChatWnd()
    {
        chatWnd.SetWndState();
    }
    public void PshChat(GameMsg msg)
    {
        chatWnd.AddChatMsg(msg.pshChat.name, msg.pshChat.chat);
    }
    #endregion

    #region Strong Wnd
    public void OpenStrongWnd()
    {
        strongWnd.SetWndState(true);
    }

    public void RspStrong(GameMsg msg)
    {
        //计算升级前的战力
        int zhanliPre = PECommon.GetFightByProps(GameRoot.Instance.PlayerData);
        //更新玩家属性数据
        GameRoot.Instance.SetPlayerDataByStrong(msg.rspStrong);
        //升级后战力
        int zhanliNow = PECommon.GetFightByProps(GameRoot.Instance.PlayerData);
        //升级后的反馈
        GameRoot.AddTips(Constants.txtColor("战力提升 " + (zhanliNow - zhanliPre), TxtColor.Blue));

        //刷新强化和主城界面
        strongWnd.UpdateUI();
        maincityWnd.RefreshUI();
    }
    #endregion

    #region Info Wnd
    public void OpenInfoWnd()
    {
        StopNavTask();

        if (charCamTrans == null)
        {
            charCamTrans = GameObject.FindGameObjectWithTag(Constants.CharShowCamWithTag).transform;
        }

        //设置人物展示相机相对位置（主角）、旋转
        charCamTrans.localPosition = Scene_player.transform.position + Scene_player.transform.forward * Constants.CharShowCamDistanceOffset + new Vector3(0, Constants.CharShowCamHeightOffset, 0);
        charCamTrans.localEulerAngles = new Vector3(0, 180 + Scene_player.transform.localEulerAngles.y, 0);
        charCamTrans.localScale = Vector3.one;
        charCamTrans.gameObject.SetActive(true);
        infoWnd.SetWndState();
    }

    public void CloseInfoWnd()
    {
        if(charCamTrans != null)
        {
            charCamTrans.gameObject.SetActive(false);
            infoWnd.SetWndState(false);
        }
    }

    private float startRoate = 0;
    public void SetStartRoate()
    {
        startRoate = Scene_player.transform.localEulerAngles.y;
    }

    public void SetPlayerRoate(float roate)
    {
        Scene_player.transform.localEulerAngles = new Vector3(0, startRoate + roate, 0);
    }
    #endregion

    public void GetMapNpcTransform()
    {
        GameObject map = GameObject.FindGameObjectWithTag(Constants.MapRootGameObjectWithTag);
        MainCityMap mainCityMap = map.GetComponent<MainCityMap>();
        npcPosTrans = mainCityMap.NpcPosTrans;
    }

    #region Guide Wnd
    private bool isNavGuide = false;
    public void RunTask(AutoGuideCfg agc)
    {
        if (agc != null)
        {
            curtTaskData = agc;
        }

        nav.enabled = true;
        //解析任务数据
        //判断是否需要寻路（找到npc）
        if (curtTaskData.npcID != -1)
        {
            float dis = Vector3.Distance(Scene_player.transform.position, npcPosTrans[agc.npcID].position); //此处的npcID与配置表guide定义的npcID一一对应
            //判断当前游戏主角与目标npc之间的距离
            if (dis < Constants.NavNpcDst)
            {
                Debug.Log("已到达目标附近，导航自动取消");
                //找到目标npc，停止导航
                isNavGuide = false;
                nav.isStopped = true;
                playerInput.move = new Vector2(0, 0);
                nav.enabled = false;

                OpenGuideWnd();
            }
            else
            {
                Debug.Log("NavMesh导航启动，自动寻路中...");
                //未找到目标npc，启动导航
                isNavGuide = true;
                nav.enabled = true; //激活导航组件
                nav.speed = Constants.PlayerMoveSpeedNav; //导航速度
                nav.SetDestination(npcPosTrans[agc.npcID].position); //设置导航目标点
                playerInput.move = new Vector2(0, 1);
            }
        }
        else
        {
            OpenGuideWnd();
        }
    }

    private void Update()
    {
        if(isNavGuide)
        {
            IsArriveNavPos();
            //playerCtrl.SetCam();
        }
    }

    private void IsArriveNavPos()
    {
        float dis = Vector3.Distance(Scene_player.transform.position, npcPosTrans[curtTaskData.npcID].position);
        if (dis < Constants.NavNpcDst)
        {
            Debug.Log("已经到达目的地，导航结束！");
            isNavGuide = false;
            nav.isStopped = true;
            playerInput.move = new Vector2(0, 0);
            nav.enabled = false;

            OpenGuideWnd();
        }
    }

    public void StopNavTask()
    {
        if (isNavGuide)
        {
            Debug.Log("因为导航中途执行其他操作，导航中断！");
            isNavGuide = false;

            nav.isStopped = true;
            nav.enabled = false;
            playerInput.move = new Vector2(0, 0);
        }
    }

    private void OpenGuideWnd()
    {
        guideWnd.SetWndState();
    }

    public AutoGuideCfg GetCurtTaskData()
    {
        return curtTaskData;
    }
    #endregion

    #region Guide Wnd
    public void OpenSettingsWnd()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        uiController.isPause = true;
        settingsWnd.SetWndState(true);

    }

    public void RspGuide(GameMsg msg)
    {
        RspGuide data = msg.rspGuide;

        GameRoot.AddTips(Constants.txtColor("任务奖励 金币+" + curtTaskData.coin + "  经验+" + curtTaskData.exp, TxtColor.Blue));

        //读取任务actionID，进行相应操作
        switch (curtTaskData.actID)
        {
            case Constants.CurtTaskDataActID_0:
                //与智者对话
                break;
            case Constants.CurtTaskDataActID_1:
                //TODO 进入副本
                break;
            case Constants.CurtTaskDataActID_2:
                //TODO 进入强化界面
                break;
            case Constants.CurtTaskDataActID_3:
                //TODO 进入体力购买
                break;
            case Constants.CurtTaskDataActID_4:
                //TODO 进入金币铸造
                break;
            case Constants.CurtTaskDataActID_5:
                //TODO 进入世界聊天
                break;
        }

        GameRoot.Instance.SetPlayerDataByGuide(data);
        maincityWnd.RefreshUI();
    }
    #endregion
}
