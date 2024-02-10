//���ܣ�����ҵ��ϵͳ
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.AI;

public class MainCitySys : SystemRoot
{
    public static MainCitySys Instance = null;

    public MainCityWnd maincityWnd;
    public InfoWnd infoWnd;
    public FpsWnd fpsWnd;
    public GameObject PlayerCameraRoot;
    private GameObject Scene_player;
    private PlayerController playerCtrl;
    private Transform charCamTrans;
    private AutoGuideCfg curtTaskData;
    private Transform[] npcPosTrans;
    private NavMeshAgent nav;

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

            //�����ǳ���UI
            maincityWnd.SetWndState();

            // ��ʼ��ҡ�˲��
            InitGamepad();

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
        //��ʼ�����λ��
        player.transform.position = mapData.playerBornPos;
        player.transform.localEulerAngles = mapData.playerBornRote;
        player.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        //�����ʼ��
        //����Ҫ�������������Ԥ�Ƽ�
        GameObject CM_player = resSvc.LoadPrefab(PathDefine.AssissnCityCharacterCameraPrefab, true);
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

        //ԭ����
        //�����ʼ��
        /*
        Camera.main.transform.position = mapData.mainCamPos;
        Camera.main.transform.localEulerAngles = mapData.mainCamRote;

        playerCtrl = player.GetComponent<PlayerController>();
        playerCtrl.Init();
        */

        nav = player.GetComponent<NavMeshAgent>();

        player.GetComponent<ThirdPersonController>().MoveSpeed = Constants.PlayerMoveSpeed;
        player.GetComponent<ThirdPersonController>().SprintSpeed = Constants.PlayerSprintSpeed;
    }

    private void InitGamepad()
    {
        GameObject GamePad = GameObject.Find(Constants.GamepadBind_StarterAssetsInputs_Joysticks);
        Scene_player = GameObject.FindGameObjectWithTag(Constants.CharPlayerWithTag);
        UICanvasControllerInput uICanvasControllerInput = GamePad.GetComponent<UICanvasControllerInput>();
        StarterAssetsInputs StarterAssetsInputs_player = Scene_player.GetComponent<StarterAssetsInputs>();

        uICanvasControllerInput.starterAssetsInputs = StarterAssetsInputs_player;
    }


    //ԭ����
    public void SetMoveDir(Vector2 dir)
    {
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


    public void OpenInfoWnd()
    {
        //��ȡ����ұ�ǩ�Ķ���
        Scene_player = GameObject.FindGameObjectWithTag(Constants.CharPlayerWithTag);
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

    public void GetMapNpcTransform()
    {
        GameObject map = GameObject.FindGameObjectWithTag(Constants.MapRootGameObjectWithTag);
        MainCityMap mainCityMap = map.GetComponent<MainCityMap>();
        npcPosTrans = mainCityMap.NpcPosTrans;
    }

    private bool isNavGuide = false;
    public void RunTask(AutoGuideCfg agc)
    {
        Scene_player = GameObject.FindGameObjectWithTag(Constants.CharPlayerWithTag);
        StarterAssetsInputs playerInput = Scene_player.GetComponent<StarterAssetsInputs>();
        if (agc != null)
        {
            curtTaskData = agc;
        }

        //������������
        //�ж��Ƿ���ҪѰ·���ҵ�npc��
        if (curtTaskData.npcID != -1)
        {
            float dis = Vector3.Distance(Scene_player.transform.position, npcPosTrans[agc.npcID].position);
            if (dis < Constants.NavNpcDst)
            {
                isNavGuide = false;
                nav.isStopped = true;
                if (nav.enabled)
                {
                    playerInput.move = new Vector2(0, 0);
                }
                nav.enabled = false;

                OpenGuideWnd();
            }
            else
            {
                isNavGuide = true;
                nav.enabled = true;
                nav.speed = Constants.PlayerMoveSpeedNav;
                nav.SetDestination(npcPosTrans[agc.npcID].position);
                playerInput.move = new Vector2(0, 1);
            }
        }
        else
        {
            OpenGuideWnd();
        }
    }

    private void OpenGuideWnd()
    {
        //TODO
    }
}
