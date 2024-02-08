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
    public InfoWnd infoWnd;
    public GameObject PlayerCameraRoot;
    private GameObject Scene_player;
    private PlayerController playerCtrl;
    private Transform charCamTrans;

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

            //��������չʾ���
            if(charCamTrans != null)
            {
                charCamTrans.gameObject.SetActive(false);
            }

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

    public void RunTask(AutoGuideCfg agc)
    {

    }

}
