namespace DarkGod.Main
{
//���ܣ�����ҵ��ϵͳ
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
    public GameObject playerInput;

    private GameObject mainCityPlayer;
    //private PlayerController playerCtrl;
    private Transform charCamTrans;
    private AutoGuideCfg curtTaskData;
    private Transform[] npcPosTrans;
    private NavMeshAgent nav;
    private StarterAssetsInputs starterAssetsInputs;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init MainCitySys...");
    }

    public void EnterMainCity()
    {
        //ͨ��id��ȡ�������ú󣬼��س���
        MapCfg mapData = resSvc.GetMapCfg(Constants.MainCityMapID);
        //�������ǳ���
        resSvc.AsyncLoadScene(mapData.sceneName, () =>
        {
            PECommon.Log("Init MainCitySys...");

            transform.Find(Constants.Path_PlayerInputs_Obj).gameObject.SetActive(true);
            // ������Ϸ����
            LoadPlayer(mapData);

            // ����NPC
            LoadNpcPrefab();

            //�����ǳ���UI
            maincityWnd.SetWndState();

            // ��ʼ��ҡ�˲��
            InitGamepad(playerInput.GetComponent<StarterAssetsInputs>());

            //���ý�ɫ����Դ
            audioSvc.GetCharacterAudioSourceComponent(mainCityPlayer);

            //�������Ǳ�������
            audioSvc.PlayBGMusic(Constants.BGMainCity);

            //��ȡ����NPCs��Transform
            GetMapNpcTransform();

            //��������չʾ���
            if (charCamTrans != null)
            {
                charCamTrans.gameObject.SetActive(false);
            }

        });

    }

    private void LoadPlayerInstance(string playerPrefabPath, MapCfg mapData)
    {
        //��ҳ�ʼ��
        //��ȡPrefabʵ�����Ķ���
        GameObject player = resSvc.LoadPrefab(playerPrefabPath, true);
        if (player != null)
        {
            //��ʼ�����λ��
            GameRoot.Instance.SetGameObjectTrans(player, mapData.playerBornPos, mapData.playerBornRote, new Vector3(0.8f, 0.8f, 0.8f));

            //ԭ����
            //�����ʼ��
            /*
            Camera.main.transform.position = mapData.mainCamPos;
            Camera.main.transform.localEulerAngles = mapData.mainCamRote;

            playerCtrl = player.GetComponent<PlayerController>();
            playerCtrl.Init();
            */

            //��ȡplayer�������
            nav = player.GetComponent<NavMeshAgent>();

            ThirdPersonController controller = player.GetComponent<ThirdPersonController>();

            controller.PlayerInput = playerInput.GetComponent<PlayerInput>();
            starterAssetsInputs = playerInput.GetComponent<StarterAssetsInputs>();
            controller.StarterAssetsInputs = starterAssetsInputs;

            controller.SetMoveMode(true);
            controller.MoveSpeed = Constants.PlayerMoveSpeed;
            controller.SprintSpeed = Constants.PlayerSprintSpeed;

            mainCityPlayer = player;
        }
    }

    private void LoadVirtualCameraInstance(string virtualCameraPrefabPath, MapCfg mapData)
    {
        //�����ʼ��
        //����Ҫ�������������Ԥ�Ƽ�
        GameObject CM_player = resSvc.LoadPrefab(virtualCameraPrefabPath, true);
        if (CM_player != null)
        {

            //����ʵ��������ʱ���λ�á���ת
            Vector3 CM_player_Pos = mapData.mainCamPos;
            Vector3 CM_player_Rote = mapData.mainCamRote;
            GameRoot.Instance.SetGameObjectTrans(CM_player, CM_player_Pos, CM_player_Rote, Vector3.one);

            // ��ȡ�������Ԥ�Ƽ��ϵ�CinemachineVirtualCamera���  
            CinemachineVirtualCamera cinemachineVirtualCamera = CM_player.GetComponent<CinemachineVirtualCamera>();
            //���ˣ�����Ӧ�û�ȡ����Ԥ�Ƽ������cinemachineVirtualCamera����Ը�ܻ�ȡ�������

            //���ˣ��������ڿ��Զ�Ԥ�Ƽ������ȡ����cinemachineVirtualCamera������в�����...>_<

            // ����CinemachineVirtualCamera�ĸ���Ŀ��Ϊ��ǩΪ"PlayerCamRoot"����Ϸ�����transform
            cinemachineVirtualCamera.Follow = GameObject.FindGameObjectWithTag(Constants.CinemachineVirtualCameraFollowGameObjectWithTag).transform;
            //ͨ����ȡ���ñ�����CinemachineVirtualCamera��ü�ƽ��
            cinemachineVirtualCamera.m_Lens.FarClipPlane = Constants.CinemachineVirtualCameraFarClipPlane;
            cinemachineVirtualCamera.m_Lens.NearClipPlane = Constants.CinemachineVirtualCameraNearClipPlane;
        }
    }

    private void LoadPlayer(MapCfg mapData)
    {
        LoadPlayerInstance(PathDefine.AssissnCityPlayerPrefab, mapData);
        LoadVirtualCameraInstance(PathDefine.AssissnCityCharacterCameraPrefab, mapData);
    }

    private void LoadNpcPrefab()
    {
        NpcCfg.Instance.LoadMapNpc(0);
        NpcCfg.Instance.LoadMapNpc(1);
        NpcCfg.Instance.LoadMapNpc(2);
        NpcCfg.Instance.LoadMapNpc(3);

    }

    private void InitGamepad(StarterAssetsInputs StarterAssetsInputs)
    {
        Transform GamePadTrans = transform.Find(Constants.Path_Joysticks_MainCitySys);
        if (GamePadTrans != null)
        {
            GamePadTrans.gameObject.SetActive(true);
            UICanvasControllerInput uICanvasControllerInput = GamePadTrans.GetComponent<UICanvasControllerInput>();

            uICanvasControllerInput.starterAssetsInputs = StarterAssetsInputs;
        }
    }


    //ԭ����
    public void SetMoveDir(Vector2 dir)
    {
        StopNavTask();
        /*
        //���ö���
        if (dir == Vector2.zero)
        {
            playerCtrl.SetBlend(Constants.BlendIdle);
        }
        else
        {
            playerCtrl.SetBlend(Constants.BlendWalk);
        }
        //���÷���
        playerCtrl.Dir = dir;
        */
    }

    #region Enter FubenSys
    public void EnterFuben()
    {
        StopNavTask();
        FubenSys.Instance.EnterFuben();
    }
    #endregion

    #region Task Wnd
    public void OpenTaskRewardWnd()
    {
        StopNavTask();
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
        StopNavTask();
        buyWnd.SetBuyType(type);
        buyWnd.SetWndState();
    }
    public void RspBuy(GameMsg msg)
    {
        RspBuy rspBuydata = msg.rspBuy;
        //����������ݵ�GameRoot��
        GameRoot.Instance.SetPlayerDataByBuy(rspBuydata);
        GameRoot.AddTips("����ɹ�");

        //�������ǽ���
        maincityWnd.RefreshUI();
        //�رչ��򴰿�
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
        StopNavTask();
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
        StopNavTask();
        strongWnd.SetWndState();
    }

    public void RspStrong(GameMsg msg)
    {
        //��������ǰ��ս��
        int zhanliPre = PECommon.GetFightByProps(GameRoot.Instance.PlayerData);
        //���������������
        GameRoot.Instance.SetPlayerDataByStrong(msg.rspStrong);
        //������ս��
        int zhanliNow = PECommon.GetFightByProps(GameRoot.Instance.PlayerData);
        //������ķ���
        GameRoot.AddTips(WindowRoot.GetTextWithHexColor("ս������ " + (zhanliNow - zhanliPre), TextColorCode.Blue));

        //ˢ��ǿ�������ǽ���
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

        //��������չʾ������λ�ã����ǣ�����ת
        charCamTrans.localPosition = mainCityPlayer.transform.position + mainCityPlayer.transform.forward * Constants.CharShowCamDistanceOffset + new Vector3(0, Constants.CharShowCamHeightOffset, 0);
        charCamTrans.localEulerAngles = new Vector3(0, 180 + mainCityPlayer.transform.localEulerAngles.y, 0);
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
        startRoate = mainCityPlayer.transform.localEulerAngles.y;
    }

    public void SetPlayerRoate(float roate)
    {
        mainCityPlayer.transform.localEulerAngles = new Vector3(0, startRoate + roate, 0);
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
        //������������
        //�ж��Ƿ���ҪѰ·���ҵ�npc��
        if (curtTaskData.npcID != -1)
        {
            float dis = Vector3.Distance(mainCityPlayer.transform.position, npcPosTrans[agc.npcID].position); //�˴���npcID�����ñ�guide�����npcIDһһ��Ӧ
            //�жϵ�ǰ��Ϸ������Ŀ��npc֮��ľ���
            if (dis < Constants.NavNpcDst)
            {
                Debug.Log("�ѵ���Ŀ�긽���������Զ�ȡ��");
                //�ҵ�Ŀ��npc��ֹͣ����
                isNavGuide = false;
                nav.isStopped = true;
                starterAssetsInputs.move = new Vector2(0, 0);
                nav.enabled = false;

                OpenGuideWnd();
            }
            else
            {
                Debug.Log("NavMesh����������Զ�Ѱ·��...");
                //δ�ҵ�Ŀ��npc���������
                isNavGuide = true;
                nav.enabled = true; //��������
                nav.speed = Constants.PlayerMoveSpeedNav; //�����ٶ�
                nav.SetDestination(npcPosTrans[agc.npcID].position); //���õ���Ŀ���
                starterAssetsInputs.move = new Vector2(0, 1);
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
        float dis = Vector3.Distance(mainCityPlayer.transform.position, npcPosTrans[curtTaskData.npcID].position);
        if (dis < Constants.NavNpcDst)
        {
            Debug.Log("�Ѿ�����Ŀ�ĵأ�����������");
            isNavGuide = false;
            nav.isStopped = true;
            starterAssetsInputs.move = new Vector2(0, 0);
            nav.enabled = false;

            OpenGuideWnd();
        }
    }

    public void StopNavTask()
    {
        if (isNavGuide)
        {
            Debug.Log("��Ϊ������;ִ�����������������жϣ�");
            isNavGuide = false;

            nav.isStopped = true;
            nav.enabled = false;
            starterAssetsInputs.move = new Vector2(0, 0);
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
        GameRoot.Instance.PauseGameUI(true);
        settingsWnd.SetWndState(true);

    }

    public void RspGuide(GameMsg msg)
    {
        RspGuide data = msg.rspGuide;

        GameRoot.AddTips(WindowRoot.GetTextWithHexColor("������ ���+" + curtTaskData.coin + "  ����+" + curtTaskData.exp, TextColorCode.Blue));

        //��ȡ����actionID��������Ӧ����
        switch (curtTaskData.actID)
        {
            case Constants.CurtTaskDataActID_0:
                //�����߶Ի�
                break;
            case Constants.CurtTaskDataActID_1:
                //���븱��
                EnterFuben();
                break;
            case Constants.CurtTaskDataActID_2:
                //����ǿ������
                OpenStrongWnd();
                break;
            case Constants.CurtTaskDataActID_3:
                //������������
                OpenBuyWnd(Constants.BuyTypePower);
                break;
            case Constants.CurtTaskDataActID_4:
                //����������
                OpenBuyWnd(Constants.MakeTypeCoin);
                break;
            case Constants.CurtTaskDataActID_5:
                //������������
                OpenChatWnd();
                break;
        }

        GameRoot.Instance.SetPlayerDataByGuide(data);
        maincityWnd.RefreshUI();
    }
    #endregion
}

}