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

    private void LoadPlayerInstance(string playerPrefabPath, MapCfg mapData)
    {
        GameObject player = resSvc.LoadPrefab(playerPrefabPath, true);
        if (player != null)
        {
            Debug.Log(playerPrefabPath + " 预制件加载成功！");
            GameRoot.Instance.SetGameObjectTrans(player, mapData.playerBornPos, mapData.playerBornRote, new Vector3(1.0f, 1.0f, 1.0f));

            ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
            controller.MoveSpeed = Constants.PlayerMoveSpeed;
            controller.SprintSpeed = Constants.PlayerSprintSpeed;
            controller.targetPlayerState = 0;

            playerInput = player.GetComponent<StarterAssetsInputs>();

            //实例化玩家逻辑实体
            EntityPlayer entitySelfPlayer = new EntityPlayer
            {
                stateMgr = stateMgr //将stateMgr注入逻辑实体类中
            };

            entitySelfPlayer.PlayerController = controller;

            Scene_player = GameObject.FindGameObjectWithTag(Constants.CharPlayerWithTag);
        }
        else
        {
            Debug.LogError(playerPrefabPath + " 预制件加载失败！");
        }
    }

    private void LoadVirtualCameraInstance(string virtualCameraPrefabPath, MapCfg mapData)
    {
        GameObject CM_player = resSvc.LoadPrefab(virtualCameraPrefabPath, true);
        if (CM_player != null)
        {
            Debug.Log(virtualCameraPrefabPath + " 预制件加载成功！");

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
            Debug.LogError(virtualCameraPrefabPath + " 预制件加载失败！");
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

            LoadPlayerInstance(PathDefine.AssissnBattlePlayerPrefab, mapData);
            //配置角色声音源
            AudioSvc.Instance.GetCharacterAudioSourceComponent();
            LoadVirtualCameraInstance(PathDefine.AssissnCityCharacterCameraPrefab, mapData);
            InitGamepad();

            audioSvc.PlayBGMusic(Constants.BGHuangYe);
        });
    }

    //设置玩家移动方向
    public void SetSelfPlayerMoveDir(Vector2 dir)
    {
        PECommon.Log(dir.ToString());
    }

    public void ReqPlayerReleaseSkill(int skillIndex)
    {
        switch(skillIndex)
        {
            case 0:
                PlayerReleaseNormalAtk();
                break;
            case 1:
                PlayerReleaseSkill01();
                break;
            case 2:
                PlayerReleaseSkill02();
                break;
            case 3:
                PlayerReleaseSkill03();
                break;
            default:
                Debug.LogError("不存在指定类型的技能，技能类型：" + skillIndex);
                break;
        }
    }

    //释放相关技能
    private void PlayerReleaseNormalAtk()
    {
        PECommon.Log("Click Normal Atk");
    }

    private void PlayerReleaseSkill01()
    {
        PECommon.Log("Click Skill01");
    }

    private void PlayerReleaseSkill02()
    {
        PECommon.Log("Click Skill02");
    }

    private void PlayerReleaseSkill03()
    {
        PECommon.Log("Click Skill03");
    }
}
