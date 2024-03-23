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
        //ͨ��id��ȡ�������ú󣬼��س���
        MapCfg mapData = resSvc.GetMapCfgCfg(Constants.MainCityMapID);
        //�������ǳ���
        resSvc.AsyncLoadScene(mapData.sceneName, () =>
        {
            PECommon.Log("Init MainCitySys...");

            // ������Ϸ����
            LoadPlayer(mapData);

            // ����NPC
            LoadNpcPrefab();

            //�����ǳ���UI
            maincityWnd.SetWndState();

            // ��ʼ��ҡ�˲��
            InitGamepad();

            //���ý�ɫ����Դ
            AudioSvc.Instance.GetCharacterAudioSourceComponent();

            //�������Ǳ�������
            audioSvc.PlayBGMusic(Constants.BGMainCity);

            //��ȡ����NPCs��Transform
            GetMapNpcTransform();

            //��������չʾ���
            if (charCamTrans != null)
            {
                charCamTrans.gameObject.SetActive(false);
            }

            //�������ܼ�����
            fpsWnd.InitWnd();

        });

    }

    private void LoadPlayer(MapCfg mapData)
    {
        //��ҳ�ʼ��
        //��ȡPrefabʵ�����Ķ���
        GameObject player = resSvc.LoadPrefab(PathDefine.AssissnCityPlayerPrefab, true);
        if (player != null)
        {
            Debug.Log(PathDefine.AssissnCityPlayerPrefab + " Ԥ�Ƽ����سɹ���");
            //��ʼ�����λ��
            GameRoot.Instance.SetGameObjectTrans(player, mapData.playerBornPos, mapData.playerBornRote, new Vector3(1.0f, 1.0f, 1.0f));

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

            player.GetComponent<ThirdPersonController>().MoveSpeed = Constants.PlayerMoveSpeed;
            player.GetComponent<ThirdPersonController>().SprintSpeed = Constants.PlayerSprintSpeed;

            playerInput = player.GetComponent<StarterAssetsInputs>();

            Scene_player = GameObject.FindGameObjectWithTag(Constants.CharPlayerWithTag);
        }
        else
        {
            Debug.LogError(PathDefine.AssissnCityPlayerPrefab + " Ԥ�Ƽ�����ʧ�ܣ�");
        }

        //�����ʼ��
        //����Ҫ�������������Ԥ�Ƽ�
        GameObject CM_player = resSvc.LoadPrefab(PathDefine.AssissnCityCharacterCameraPrefab, true);
        if (CM_player != null)
        {
            Debug.Log(PathDefine.AssissnCityCharacterCameraPrefab + " Ԥ�Ƽ����سɹ���");
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
        else
        {
            Debug.LogError(PathDefine.AssissnCityCharacterCameraPrefab + " Ԥ�Ƽ�����ʧ�ܣ�");
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


    //ԭ����
    public void SetMoveDir(Vector2 dir)
    {
        StopNavTask();

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
        //��������ǰ��ս��
        int zhanliPre = PECommon.GetFightByProps(GameRoot.Instance.PlayerData);
        //���������������
        GameRoot.Instance.SetPlayerDataByStrong(msg.rspStrong);
        //������ս��
        int zhanliNow = PECommon.GetFightByProps(GameRoot.Instance.PlayerData);
        //������ķ���
        GameRoot.AddTips(Constants.txtColor("ս������ " + (zhanliNow - zhanliPre), TxtColor.Blue));

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
        //������������
        //�ж��Ƿ���ҪѰ·���ҵ�npc��
        if (curtTaskData.npcID != -1)
        {
            float dis = Vector3.Distance(Scene_player.transform.position, npcPosTrans[agc.npcID].position); //�˴���npcID�����ñ�guide�����npcIDһһ��Ӧ
            //�жϵ�ǰ��Ϸ������Ŀ��npc֮��ľ���
            if (dis < Constants.NavNpcDst)
            {
                Debug.Log("�ѵ���Ŀ�긽���������Զ�ȡ��");
                //�ҵ�Ŀ��npc��ֹͣ����
                isNavGuide = false;
                nav.isStopped = true;
                playerInput.move = new Vector2(0, 0);
                nav.enabled = false;

                OpenGuideWnd();
            }
            else
            {
                Debug.Log("NavMesh�����������Զ�Ѱ·��...");
                //δ�ҵ�Ŀ��npc����������
                isNavGuide = true;
                nav.enabled = true; //��������
                nav.speed = Constants.PlayerMoveSpeedNav; //�����ٶ�
                nav.SetDestination(npcPosTrans[agc.npcID].position); //���õ���Ŀ���
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
            Debug.Log("�Ѿ�����Ŀ�ĵأ�����������");
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
            Debug.Log("��Ϊ������;ִ�����������������жϣ�");
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

        GameRoot.AddTips(Constants.txtColor("������ ���+" + curtTaskData.coin + "  ����+" + curtTaskData.exp, TxtColor.Blue));

        //��ȡ����actionID��������Ӧ����
        switch (curtTaskData.actID)
        {
            case Constants.CurtTaskDataActID_0:
                //�����߶Ի�
                break;
            case Constants.CurtTaskDataActID_1:
                //TODO ���븱��
                break;
            case Constants.CurtTaskDataActID_2:
                //TODO ����ǿ������
                break;
            case Constants.CurtTaskDataActID_3:
                //TODO ������������
                break;
            case Constants.CurtTaskDataActID_4:
                //TODO ����������
                break;
            case Constants.CurtTaskDataActID_5:
                //TODO ������������
                break;
        }

        GameRoot.Instance.SetPlayerDataByGuide(data);
        maincityWnd.RefreshUI();
    }
    #endregion
}
