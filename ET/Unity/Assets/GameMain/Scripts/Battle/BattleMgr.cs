//功能：战场管理器


using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class BattleMgr : MonoBehaviour
{
    private ResSvc resSvc;
    private AudioSvc audioSvc;

    private StateMgr stateMgr;
    private SkillMgr skillMgr;
    private MapMgr mapMgr;

    private GameObject Scene_player;
    private StarterAssetsInputs playerInput;

    private void LoadPlayerInstance(MapCfg mapData)
    {
        GameObject player = resSvc.LoadPrefab(PathDefine.AssissnCityPlayerPrefab, true);
        if (player != null)
        {
            Debug.Log(PathDefine.AssissnCityPlayerPrefab + " 预制件加载成功！");
            GameRoot.Instance.SetGameObjectTrans(player, mapData.playerBornPos, mapData.playerBornRote, new Vector3(1.0f, 1.0f, 1.0f));

            player.GetComponent<ThirdPersonController>().MoveSpeed = Constants.PlayerMoveSpeed;
            player.GetComponent<ThirdPersonController>().SprintSpeed = Constants.PlayerSprintSpeed;
            player.GetComponent<ThirdPersonController>().isSkillMove = true;

            playerInput = player.GetComponent<StarterAssetsInputs>();

            Scene_player = GameObject.FindGameObjectWithTag(Constants.CharPlayerWithTag);
        }
        else
        {
            Debug.LogError(PathDefine.AssissnCityPlayerPrefab + " 预制件加载失败！");
        }
    }

    private void LoadVirtualCameraInstance(MapCfg mapData)
    {
        GameObject CM_player = resSvc.LoadPrefab(PathDefine.AssissnCityCharacterCameraPrefab, true);
        if (CM_player != null)
        {
            Debug.Log(PathDefine.AssissnCityCharacterCameraPrefab + " 预制件加载成功！");

            Vector3 CM_player_Pos = mapData.mainCamPos;
            Vector3 CM_player_Rote = mapData.mainCamRote;
            GameRoot.Instance.SetGameObjectTrans(CM_player, CM_player_Pos, CM_player_Rote, Vector3.one);

            CinemachineVirtualCamera cinemachineVirtualCamera = CM_player.GetComponent<CinemachineVirtualCamera>();

            cinemachineVirtualCamera.Follow = GameObject.FindGameObjectWithTag(Constants.CinemachineVirtualCameraFollowGameObjectWithTag).transform;

            cinemachineVirtualCamera.m_Lens.FarClipPlane = Constants.CinemachineVirtualCameraFarClipPlane;
            cinemachineVirtualCamera.m_Lens.NearClipPlane = Constants.CinemachineVirtualCameraNearClipPlane;
        }
        else
        {
            Debug.LogError(PathDefine.AssissnCityCharacterCameraPrefab + " 预制件加载失败！");
        }
    }

    private void InitGamepad()
    {
        Transform GamePadTrans = transform.Find(Constants.Path_Joysticks_BattleMgr);
        if (GamePadTrans != null)
        {
            GamePadTrans.gameObject.SetActive(true);
            UICanvasControllerInput uICanvasControllerInput = GamePadTrans.GetComponent<UICanvasControllerInput>();
            StarterAssetsInputs StarterAssetsInputs_player = Scene_player.GetComponent<StarterAssetsInputs>();

            uICanvasControllerInput.starterAssetsInputs = StarterAssetsInputs_player;
        }
    }

    public void Init(int mapid)
    {
        //初始化服务模块
        resSvc = ResSvc.Instance;
        audioSvc = AudioSvc.Instance;

        //初始化各管理器
        stateMgr = gameObject.AddComponent<StateMgr>();
        stateMgr.Init();
        skillMgr = gameObject.AddComponent<SkillMgr>();
        skillMgr.Init();

        //加载战场地图
        MapCfg mapData = resSvc.GetMapCfg(mapid);
        resSvc.AsyncLoadScene(mapData.sceneName, () =>
        {
            //初始化地图数据
            GameObject mapRoot = GameObject.FindGameObjectWithTag(Constants.MapRootGOTag);
            mapMgr = mapRoot.GetComponent<MapMgr>();
            mapMgr.Init();

            GameRoot.Instance.SetGameObjectTrans(mapRoot, Vector3.zero, Vector3.zero, Vector3.one);

            LoadPlayerInstance(mapData);
            //配置角色声音源
            AudioSvc.Instance.GetCharacterAudioSourceComponent();
            LoadVirtualCameraInstance(mapData);
            InitGamepad();

            audioSvc.PlayBGMusic(Constants.BGHuangYe);
        });
    }
}
