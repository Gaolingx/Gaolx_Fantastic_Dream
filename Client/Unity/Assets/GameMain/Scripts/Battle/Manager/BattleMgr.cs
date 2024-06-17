//功能：战场管理器


using Cinemachine;
using PEProtocol;
using StarterAssets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace DarkGod.Main
{
    public class BattleMgr : MonoBehaviour
    {
        private ResSvc resSvc;
        private AudioSvc audioSvc;
        private TimerSvc timerSvc;

        private StateMgr stateMgr;
        private SkillMgr skillMgr;
        private MapMgr mapMgr;

        private BattleSys battleSys;

        private EntityPlayer entitySelfPlayer;
        public EntityPlayer EntityPlayer { get { return entitySelfPlayer; } set { SetEntityPlayer(value); entitySelfPlayer = value; } }
        private ThirdPersonController controller;
        private StarterAssetsInputs starterAssetsInputs;
        private MapCfg mapCfg;

        private Dictionary<string, EntityMonster> monsterDic = new Dictionary<string, EntityMonster>();

        private void InitPlayerInput()
        {
            starterAssetsInputs = GameRoot.Instance.GetStarterAssetsInputs();
        }


        CinemachineVirtualCamera cinemachineVirtualCamera;
        private async void LoadVirtualCameraInstance(string virtualCameraPrefabPath, MapCfg mapData)
        {
            Vector3 CM_player_Pos = mapData.mainCamPos;
            Vector3 CM_player_Rote = mapData.mainCamRote;
            GameObject CM_player = await resSvc.LoadGameObjectAsync(virtualCameraPrefabPath, CM_player_Pos, CM_player_Rote, Vector3.one);

            if (CM_player != null)
            {
                cinemachineVirtualCamera = CM_player.GetComponent<CinemachineVirtualCamera>();

                cinemachineVirtualCamera.m_Lens.FarClipPlane = Constants.CinemachineVirtualCameraFarClipPlane;
                cinemachineVirtualCamera.m_Lens.NearClipPlane = Constants.CinemachineVirtualCameraNearClipPlane;
            }
        }

        private async void LoadPlayerInstance(string playerPrefabPath, MapCfg mapData)
        {
            GameObject player = await resSvc.LoadGameObjectAsync(playerPrefabPath, mapData.playerBornPos, mapData.playerBornRote, new Vector3(0.8f, 0.8f, 0.8f));

            if (player != null)
            {
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

                controller.PlayerInput = starterAssetsInputs.gameObject.GetComponent<PlayerInput>();
                controller.StarterAssetsInputs = starterAssetsInputs;

                controller.MoveSpeed = Constants.PlayerMoveSpeed;
                controller.SprintSpeed = Constants.PlayerSprintSpeed;
                controller.SetMoveMode(false);
                controller.playerFollowVirtualCamera = cinemachineVirtualCamera;
                entitySelfPlayer.playerController = controller;

                entitySelfPlayer.playerInput = starterAssetsInputs;

                //配置角色声音源
                GameRoot.Instance.SetAudioListener(player.GetComponent<AudioListener>(), true, false);
                audioSvc.GetCharacterAudioSourceComponent(player);

                cinemachineVirtualCamera.Follow = player.transform.Find(Constants.CinemachineVirtualCameraFollowGameObjectWithTag);

                EntityPlayer = entitySelfPlayer;
            }
        }

        public void Init(int mapid, Action cb = null)
        {
            InitPlayerInput();

            //初始化服务模块
            resSvc = ResSvc.Instance;
            audioSvc = AudioSvc.Instance;
            timerSvc = TimerSvc.Instance;

            //初始化系统
            battleSys = BattleSys.Instance;

            //初始化各管理器
            stateMgr = gameObject.AddComponent<StateMgr>();
            stateMgr.Init();
            skillMgr = gameObject.AddComponent<SkillMgr>();
            skillMgr.Init();

            //加载战场地图
            mapCfg = resSvc.GetMapCfg(mapid);
            resSvc.AsyncLoadScene(mapCfg.sceneName, () =>
            {
                //移除所有实例化的对象
                resSvc.DestroyAllInstantiateGameObject();

                //初始化地图数据
                GameObject mapRoot = GameObject.FindGameObjectWithTag(Constants.MapRootGOTag);
                mapMgr = mapRoot.GetComponent<MapMgr>();
                mapMgr.Init(this);

                GameRoot.Instance.SetGameObjectTrans(mapRoot, Vector3.zero, Vector3.zero, Vector3.one);

                //加载虚拟相机
                LoadVirtualCameraInstance(PathDefine.AssissnCityCharacterCameraPrefab, mapCfg);

                //加载玩家实体
                LoadPlayerInstance(PathDefine.AssissnBattlePlayerPrefab, mapCfg);
                entitySelfPlayer.StateIdle();

                //延迟激活第一批次怪物
                ActiveCurrentBatchMonsters();

                //切换BGM
                audioSvc.PlayBGMusic(Constants.BGHuangYe);

                SetPauseGame(false, false);

                if (cb != null)
                {
                    cb();
                }
            });
        }

        //相关逻辑驱动
        private bool isPauseGameAI = false;
        public void SetPauseGame(bool stateUI, bool stateAI)
        {
            isPauseGameAI = stateAI;
            GameRoot.Instance.PauseGameUI(stateUI);
        }
        public bool GetPauseGame()
        {
            return isPauseGameAI;
        }

        private void Update()
        {
            CheckMonsterCount();

            RunMonsterAILogic();
        }

        private void RunMonsterAILogic()
        {
            foreach (var item in monsterDic)
            {
                EntityMonster em = item.Value;
                em.TickAILogic();
            }
        }

        private bool triggerCheck = true;
        public void SetTriggerCheck(bool state)
        {
            triggerCheck = state;
        }
        public bool GetTriggerCheck()
        {
            return triggerCheck;
        }

        private void CheckMonsterCount()
        {
            //检测当前批次的怪物是否全部死亡
            if (mapMgr != null)
            {
                if (triggerCheck && monsterDic.Count == 0)
                {
                    bool isExist = mapMgr.SetNextTriggerOn();
                    triggerCheck = false;
                    if (!isExist)
                    {
                        //关卡结束，战斗胜利
                        EndBattle(true, entitySelfPlayer.HP);
                    }
                }
            }
        }

        //战斗结算处理
        public void EndBattle(bool isWin, int restHP)
        {
            SetPauseGame(false, true);
            //停止背景音乐
            audioSvc.StopBGMusic();
            battleSys.EndBattle(isWin, restHP);
        }

        //通过批次ID生成怪物
        public async void LoadMonsterByWaveID(int wave)
        {
            for (int i = 0; i < mapCfg.monsterLst.Count; i++)
            {
                MonsterData md = mapCfg.monsterLst[i];
                //判断是否为对应批次的怪物，是则实例化
                if (md.mWave == wave)
                {
                    GameObject m = await resSvc.LoadGameObjectAsync(md.mCfg.resPath, md.mBornPos, md.mBornRote, Vector3.one);

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
                    mc.MonsterMoveSpeed = md.mMoveSpeed;
                    mc.EnableDownSpeed = true;
                    em.SetCtrl(mc);

                    m.SetActive(false);
                    monsterDic.Add(m.name, em);
                    //Boss血条特殊处理
                    if (md.mCfg.mType == MonsterType.Normal)
                    {
                        GameRoot.Instance.dynamicWnd.AddHpItemInfo(m.name, mc.hpRoot, em.HP);
                    }
                    else if (md.mCfg.mType == MonsterType.Boss)
                    {
                        battleSys.playerCtrlWnd.SetBossHPBarState(true);
                    }
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
            if (monsterDic.TryGetValue(key, out entityMonster))
            {
                //移除数据
                monsterDic.Remove(key);
                //移除血条
                GameRoot.Instance.dynamicWnd.RmvHpItemInfo(key);
            }
        }

        #region 技能施放与角色控制
        public void SetSelfPlayerMoveDir(Vector2 dir)
        {
            //设置玩家移动
            //PECommon.Log(dir.ToString());
            if (entitySelfPlayer.CanControl == false)
            {
                GameRoot.Instance.EnableInputAction(false);
                return;
            }
            else
            {
                GameRoot.Instance.EnableInputAction(true);
            }

            //判断动画状态
            if (entitySelfPlayer.currentAniState == AniState.Idle || entitySelfPlayer.currentAniState == AniState.Move)
            {
                if (dir == Vector2.zero)
                {
                    entitySelfPlayer.StateIdle();
                }
                else
                {
                    entitySelfPlayer.StateMove();
                    entitySelfPlayer.SetDir(dir);
                }
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
        public double lastAtkTime = 0; //上一次释放普攻时间
        private int[] comboArr = Constants.comboArr01; //普攻连招技能id
        public int comboIndex = 0; //记录当前要存储的连招的id为第n个
        private void CalcNormalAtkCombo()
        {
            if (entitySelfPlayer.currentAniState == AniState.Attack)
            {
                //在500ms以内进行第二次点击，保存点击数据
                double nowAtkTime = timerSvc.GetNowTime();
                if (nowAtkTime - lastAtkTime < Constants.ComboSpace01 && lastAtkTime != 0)
                {
                    //防止数组越界
                    if (comboArr[comboIndex] != comboArr[comboArr.Length - 1])
                    {
                        comboIndex += 1;
                        entitySelfPlayer.comboQue.Enqueue(comboArr[comboIndex]); //记录连招id
                        lastAtkTime = nowAtkTime;
                    }
                    else
                    {
                        //如果连招已经注册满，重置
                        lastAtkTime = 0;
                        comboIndex = 0;
                    }
                }
            }
            else if (entitySelfPlayer.currentAniState == AniState.Idle ||
                entitySelfPlayer.currentAniState == AniState.Move)
            {
                comboIndex = 0;
                lastAtkTime = timerSvc.GetNowTime();
                entitySelfPlayer.StateAttack(comboArr[comboIndex]);
            }
        }

        private void PlayerReleaseNormalAtk()
        {
            //PECommon.Log("Click Normal Atk");
            CalcNormalAtkCombo();
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
            return battleSys.GetDirInput();
        }
        public bool CanRlsSkill()
        {
            return entitySelfPlayer.CanRlsSkill;
        }

        #endregion

        public void SetEntityPlayer(EntityPlayer player)
        {
            GameRoot.Instance.SetCurrentPlayer(player);
        }

    }
}
