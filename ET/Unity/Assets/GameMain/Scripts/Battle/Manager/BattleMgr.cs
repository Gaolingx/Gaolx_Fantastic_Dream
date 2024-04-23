//功能：战场管理器


using Cinemachine;
using PEProtocol;
using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class BattleMgr : MonoBehaviour
{
    private ResSvc resSvc;
    private AudioSvc audioSvc;
    private TimerSvc timerSvc;

    private StateMgr stateMgr;
    private SkillMgr skillMgr;
    private MapMgr mapMgr;

    private EntityPlayer entitySelfPlayer;
    private ThirdPersonController controller;
    private StarterAssetsInputs starterAssetsInputs;
    private GameObject battlePlayer;
    private MapCfg mapCfg;
    public Transform playerInputObj;

    private Dictionary<string, EntityMonster> monsterDic = new Dictionary<string, EntityMonster>();

    private void LoadPlayerInstance(string playerPrefabPath, MapCfg mapData)
    {
        GameObject player = resSvc.LoadPrefab(playerPrefabPath, true);
        if (player != null)
        {
            Debug.Log(playerPrefabPath + " 预制件加载成功！");
            GameRoot.Instance.SetGameObjectTrans(player, mapData.playerBornPos, mapData.playerBornRote, new Vector3(0.8f, 0.8f, 0.8f));

            PlayerData pd = GameRoot.Instance.PlayerData;
            BattleProps props = new BattleProps
            {
                hp = pd.hp,
                ad = pd.ad,
                ap = pd.ap,
                addef = pd.addef,
                apdef = pd.apdef,
                dodge = pd.dodge,
                pierce = pd.pierce,
                critical = pd.critical
            };

            //实例化玩家逻辑实体
            entitySelfPlayer = new EntityPlayer
            {
                battleMgr = this,
                stateMgr = stateMgr, //将stateMgr注入逻辑实体类中
                skillMgr = skillMgr
            };
            entitySelfPlayer.Name = pd.name;
            entitySelfPlayer.SetBattleProps(props);

            controller = player.GetComponent<ThirdPersonController>();

            controller.PlayerInput = playerInputObj.gameObject.GetComponent<PlayerInput>();

            starterAssetsInputs = playerInputObj.gameObject.GetComponent<StarterAssetsInputs>();
            controller.StarterAssetsInputs = starterAssetsInputs;

            controller.MoveSpeed = Constants.PlayerMoveSpeed;
            controller.SprintSpeed = Constants.PlayerSprintSpeed;
            controller.SetAniBlend(Constants.State_Mar7th00_Blend_Idle);
            entitySelfPlayer.playerController = controller;

            entitySelfPlayer.playerInput = starterAssetsInputs;

            battlePlayer = player;
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

    public void Init(int mapid)
    {

        //初始化服务模块
        resSvc = ResSvc.Instance;
        audioSvc = AudioSvc.Instance;
        timerSvc = TimerSvc.Instance;

        //初始化各管理器
        stateMgr = gameObject.AddComponent<StateMgr>();
        stateMgr.Init();
        skillMgr = gameObject.AddComponent<SkillMgr>();
        skillMgr.Init();

        //加载战场地图
        mapCfg = resSvc.GetMapCfg(mapid);
        resSvc.AsyncLoadScene(mapCfg.sceneName, () =>
        {
            //初始化地图数据
            GameObject mapRoot = GameObject.FindGameObjectWithTag(Constants.MapRootGOTag);
            mapMgr = mapRoot.GetComponent<MapMgr>();
            mapMgr.Init(this);

            GameRoot.Instance.SetGameObjectTrans(mapRoot, Vector3.zero, Vector3.zero, Vector3.one);

            LoadPlayerInstance(PathDefine.AssissnBattlePlayerPrefab, mapCfg);
            entitySelfPlayer.StateIdle();

            LoadVirtualCameraInstance(PathDefine.AssissnCityCharacterCameraPrefab, mapCfg);

            //延迟激活第一批次怪物
            ActiveCurrentBatchMonsters();

            //配置角色声音源
            audioSvc.GetCharacterAudioSourceComponent(battlePlayer);
            audioSvc.PlayBGMusic(Constants.BGHuangYe);

            SetEntityPlayer(entitySelfPlayer);
        });
    }

    //通过批次ID生成怪物
    public void LoadMonsterByWaveID(int wave)
    {
        for (int i = 0; i < mapCfg.monsterLst.Count; i++)
        {
            MonsterData md = mapCfg.monsterLst[i];
            //判断是否为对应批次的怪物，是则实例化
            if (md.mWave == wave)
            {
                GameObject m = resSvc.LoadPrefab(md.mCfg.resPath, true);
                GameRoot.Instance.SetGameObjectTrans(m, md.mBornPos, md.mBornRote, Vector3.one);

                m.name = "m" + md.mWave + "_" + md.mIndex;

                EntityMonster em = new EntityMonster
                {
                    battleMgr = this,
                    stateMgr = stateMgr, //将stateMgr注入逻辑实体类中
                    skillMgr = skillMgr
                };
                //设置初始属性
                em.md = md;
                em.SetBattleProps(md.mCfg.bps);
                em.Name = m.name;

                MonsterController mc = m.GetComponent<MonsterController>();
                mc.Init();
                em.SetCtrl(mc);

                m.SetActive(false);
                monsterDic.Add(m.name, em);
                GameRoot.Instance.dynamicWnd.AddHpItemInfo(m.name, mc.hpRoot, em.HP);
            }
        }
    }

    //延迟激活当前批次怪物
    public void ActiveCurrentBatchMonsters()
    {
        timerSvc.AddTimeTask((int tid) =>
        {
            foreach (var item in monsterDic)
            {
                item.Value.SetActive(true);
                //进入Born状态
                item.Value.StateBorn();
                timerSvc.AddTimeTask((int tid1) =>
                {
                    //出生1秒钟后进入Idle状态
                    item.Value.StateIdle();
                }, Constants.StateIdleMonsterDelayTime);
            }
        }, Constants.ActiveMonsterDelayTime);
    }

    //获取所有怪物实体
    public List<EntityMonster> GetEntityMonsters()
    {
        List<EntityMonster> monsterLst = new List<EntityMonster>();
        foreach (var item in monsterDic)
        {
            monsterLst.Add(item.Value);
        }
        return monsterLst;
    }

    public void RmvMonster(string key)
    {
        EntityMonster entityMonster;
        if(monsterDic.TryGetValue(key, out entityMonster))
        {
            //移除数据
            monsterDic.Remove(key);
            //移除血条
            GameRoot.Instance.dynamicWnd.RmvHpItemInfo(key);
        }
    }

    #region 技能施放与角色控制
    //设置玩家移动方向
    public void SetSelfPlayerMoveDir(Vector2 dir)
    {
        //PECommon.Log(dir.ToString());

        if (dir == Vector2.zero)
        {
            entitySelfPlayer.StateIdle();
        }
        else
        {
            entitySelfPlayer.StateMove();
        }
    }
    public void ReqPlayerReleaseSkill(int skillIndex)
    {
        switch (skillIndex)
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
        //PECommon.Log("Click Normal Atk");
        entitySelfPlayer.StateAttack(Constants.SkillID_Mar7th00_normalAtk01);
    }
    private void PlayerReleaseSkill01()
    {
        //PECommon.Log("Click Skill01");
        entitySelfPlayer.StateAttack(Constants.SkillID_Mar7th00_skill01);
    }
    private void PlayerReleaseSkill02()
    {
        //PECommon.Log("Click Skill02");
        entitySelfPlayer.StateAttack(Constants.SkillID_Mar7th00_skill02);
    }
    private void PlayerReleaseSkill03()
    {
        //PECommon.Log("Click Skill03");
        entitySelfPlayer.StateAttack(Constants.SkillID_Mar7th00_skill03);
    }
    public Vector2 GetDirInput()
    {
        return BattleSys.Instance.GetDirInput();
    }
    public void SetEntityPlayer(EntityPlayer player)
    {
        GameRoot.Instance.SetCurrentPlayer(player);
    }
    #endregion
}
