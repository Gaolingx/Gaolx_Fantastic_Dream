//���ܣ�����ҵ��ϵͳ
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;

public class MainCitySys : SystemRoot
{
    public static MainCitySys Instance = null;

    public MainCityWnd maincityWnd;
    public GameObject PlayerCameraRoot;
    private PlayerController playerCtrl;

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

            //TODO ��������չʾ���

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
        player.transform.localScale = new Vector3(1.0f,1.0f,1.0f);

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

    }

    private void InitGamepad()
    {
        GameObject GamePad = GameObject.Find(Constants.GamepadBind_StarterAssetsInputs_Joysticks);
        GameObject Gamepad_player = GameObject.Find(Constants.GamePadBind_Player);
        UICanvasControllerInput uICanvasControllerInput = GamePad.GetComponent<UICanvasControllerInput>();
        StarterAssetsInputs StarterAssetsInputs_player = Gamepad_player.GetComponent<StarterAssetsInputs>();
        
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

}
