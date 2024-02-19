//���ܣ�����ҵ��ϵͳ
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using PEProtocol;

public class MainCitySys : SystemRoot
{
    public static MainCitySys Instance = null;

    public MainCityWnd maincityWnd;
    public InfoWnd infoWnd;
    public GuideWnd guideWnd;
    public StrongWnd strongWnd;
    public SettingsWnd settingsWnd;
    public FpsWnd fpsWnd;

    public GameObject PlayerCameraRoot;
    private GameObject Scene_player;
    private PlayerController playerCtrl;
    private Transform charCamTrans;
    private AutoGuideCfg curtTaskData;
    private Transform[] npcPosTrans;
    private NavMeshAgent nav;
    StarterAssetsInputs playerInput;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init MainCitySys...");
    }

    public void EnterMainCity()
    {
        //ͨ��id��ȡ�������ú󣬼��س���
        MapCfg mapData = resSvc.GetMapCfgData(Constants.MainCityMapID);
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
            Debug.Log("PlayerԤ�Ƽ����سɹ���");
            //��ʼ�����λ��
            player.transform.position = mapData.playerBornPos;
            player.transform.localEulerAngles = mapData.playerBornRote;
            player.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

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
            Debug.LogError("PlayerԤ�Ƽ�����ʧ�ܣ�");
        }

        //�����ʼ��
        //����Ҫ�������������Ԥ�Ƽ�
        GameObject CM_player = resSvc.LoadPrefab(PathDefine.AssissnCityCharacterCameraPrefab, true);
        if (CM_player != null)
        {
            Debug.Log("PlayerFollowCameraԤ�Ƽ����سɹ���");
            //����ʵ��������ʱ���λ�á���ת
            Vector3 CM_player_Pos = mapData.mainCamPos;
            Vector3 CM_player_Rote = mapData.mainCamRote;
            CM_player.transform.position = CM_player_Pos;
            CM_player.transform.localEulerAngles = CM_player_Rote;

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
            Debug.LogError("PlayerFollowCameraԤ�Ƽ�����ʧ�ܣ�");
        }

    }

    private void LoadNpcPrefab()
    {
        GameObject NPC0 = resSvc.LoadPrefab(PathDefine.AssissnCityNPC0Prefab, true);
        NPC0.transform.position = NpcCfg.Instance.Transform_NpcID_0_Position;
        NPC0.transform.localEulerAngles = NpcCfg.Instance.Transform_NpcID_0_Rotation;
        NPC0.transform.localScale = NpcCfg.Instance.Transform_NpcID_0_Scale;
        Debug.Log("NPC_0Ԥ�Ƽ����سɹ���");

        GameObject NPC1 = resSvc.LoadPrefab(PathDefine.AssissnCityNPC1Prefab, true);
        NPC1.transform.position = NpcCfg.Instance.Transform_NpcID_1_Position;
        NPC1.transform.localEulerAngles = NpcCfg.Instance.Transform_NpcID_1_Rotation;
        NPC1.transform.localScale = NpcCfg.Instance.Transform_NpcID_1_Scale;
        Debug.Log("NPC_1Ԥ�Ƽ����سɹ���");

        GameObject NPC2 = resSvc.LoadPrefab(PathDefine.AssissnCityNPC2Prefab, true);
        NPC2.transform.position = NpcCfg.Instance.Transform_NpcID_2_Position;
        NPC2.transform.localEulerAngles = NpcCfg.Instance.Transform_NpcID_2_Rotation;
        NPC2.transform.localScale = NpcCfg.Instance.Transform_NpcID_2_Scale;
        Debug.Log("NPC_2Ԥ�Ƽ����سɹ���");

        GameObject NPC3 = resSvc.LoadPrefab(PathDefine.AssissnCityNPC3Prefab, true);
        NPC3.transform.position = NpcCfg.Instance.Transform_NpcID_3_Position;
        NPC3.transform.localEulerAngles = NpcCfg.Instance.Transform_NpcID_3_Rotation;
        NPC3.transform.localScale = NpcCfg.Instance.Transform_NpcID_3_Scale;
        Debug.Log("NPC_3Ԥ�Ƽ����سɹ���");


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

    #region Strong Wnd
    public void OpenStrongWnd()
    {
        strongWnd.SetWndState(true);

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
