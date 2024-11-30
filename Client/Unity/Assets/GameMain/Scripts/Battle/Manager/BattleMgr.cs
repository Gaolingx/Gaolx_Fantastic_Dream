//功能：战场管理器

using Cinemachine;
using Cysharp.Threading.Tasks;
using PEProtocol;
using StarterAssets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarkGod.Main
{
    public class BattleMgr : MonoBehaviour
    {
        private ResSvc resSvc;
        private ConfigSvc configSvc;
        private AudioSvc audioSvc;
        private TimerSvc timerSvc;

        private StateMgr stateMgr;
        private SkillMgr skillMgr;
        private VFXManager VFXMgr;
        private MapMgr mapMgr;

        private BattleSys battleSys;

        private MapCfg mapCfg;

        public Dictionary<string, EntityMonster> monsterDic = new Dictionary<string, EntityMonster>();
        public Dictionary<string, EntityPlayer> playerDic = new Dictionary<string, EntityPlayer>();

        private async UniTask<CinemachineVirtualCamera> LoadVirtualCameraInstance(MapCfg mapData)
        {
            Vector3 CM_player_Pos = mapData.mainCamPos;
            Vector3 CM_player_Rote = mapData.mainCamRote;
            GameObject CM_player = await resSvc.LoadGameObjectAsync(Constants.ResourcePackgeName, mapData.playerCamPath, CM_player_Pos, Quaternion.Euler(CM_player_Rote), Vector3.one, true, false, false);

            CinemachineVirtualCamera cinemachineVirtualCamera = null;
            if (CM_player != null)
            {
                cinemachineVirtualCamera = CM_player.GetComponent<CinemachineVirtualCamera>();

                cinemachineVirtualCamera.m_Lens.FarClipPlane = Constants.CinemachineVirtualCameraFarClipPlane;
                cinemachineVirtualCamera.m_Lens.NearClipPlane = Constants.CinemachineVirtualCameraNearClipPlane;
            }

            return cinemachineVirtualCamera;
        }

        public void ActiveCurrentPlayer(string key)
        {
            timerSvc.AddTimeTask((int tid) =>
            {
                if (playerDic.TryGetValue(key, out EntityPlayer item))
                {
                    EventMgr.OnEntityPlayerChangedEvent.SendEventMessage(item);
                    item.SetActive(true);
                    item.StateBorn();
                    timerSvc.AddTimeTask((int tid1) =>
                    {
                        item.StateIdle();
                    }, Constants.StateIdlePlayerDelayTime);
                }
            }, Constants.ActivePlayerDelayTime);
        }

        private async void LoadPlayerInstance(string playerPath, Vector3 playerBornPos, Vector3 playerBornRote, Vector3 playerBornScale, CinemachineVirtualCamera cinemachineVirtualCamera)
        {
            GameObject player = await resSvc.LoadGameObjectAsync(Constants.ResourcePackgeName, playerPath, playerBornPos, Quaternion.Euler(playerBornRote), playerBornScale, true, false, false);

            if (player != null)
            {
                ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
                StarterAssetsInputs starterAssetsInputs = InputMgr.MainInstance.starterAssetsInputs;

                controller.PlayerInput = starterAssetsInputs.GetComponent<PlayerInput>();
                controller.StarterAssetsInputs = starterAssetsInputs;
                controller.playerFollowVirtualCamera = cinemachineVirtualCamera;

                controller.SetMoveMode(ThirdPersonController.ControlState.Manual);
                controller.MoveSpeed = Constants.PlayerMoveSpeed;
                controller.SprintSpeed = Constants.PlayerSprintSpeed;


                cinemachineVirtualCamera.Follow = player.transform.Find(Constants.CinemachineVirtualCameraFollowGameObjectWithTag);

                PlayerData pd = GameRoot.MainInstance.PlayerData;
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
                EntityPlayer entitySelfPlayer = new EntityPlayer
                {
                    battleMgr = this,
                    stateMgr = stateMgr, //将stateMgr注入逻辑实体类中
                    skillMgr = skillMgr,
                    VFXMgr = VFXMgr,
                };
                entitySelfPlayer.EntityName = pd.name;
                entitySelfPlayer.AddEntityEventListener();
                entitySelfPlayer.SetBattleProps(props);

                entitySelfPlayer.SetCtrl(controller);
                entitySelfPlayer.SetActive(false);
                playerDic.Add(entitySelfPlayer.EntityName, entitySelfPlayer);
                entitySelfPlayer.OnInitFSM();
                ActiveCurrentPlayer(entitySelfPlayer.EntityName);
            }
        }

        public void Init(int mapid, System.Action cb = null)
        {
            //初始化服务模块
            resSvc = ResSvc.MainInstance;
            configSvc = ConfigSvc.MainInstance;
            audioSvc = AudioSvc.MainInstance;
            timerSvc = TimerSvc.MainInstance;

            //初始化系统
            battleSys = BattleSys.MainInstance;

            //初始化各管理器
            VFXMgr = VFXManager.MainInstance;
            stateMgr = gameObject.AddComponent<StateMgr>();
            skillMgr = gameObject.AddComponent<SkillMgr>();
            skillMgr.Init();

            //加载战场地图
            mapCfg = configSvc.GetMapCfg(mapid);
            resSvc.AsyncLoadScene(Constants.ResourcePackgeName, mapCfg.sceneName, async () =>
            {
                //初始化地图数据
                mapMgr = GameObject.FindGameObjectWithTag(Constants.MapRootWithTag_Battle).GetComponent<MapMgr>();
                mapMgr.Init(this);

                AddEntityPlayerData();

                //加载虚拟相机
                CinemachineVirtualCamera virtualCamera = await LoadVirtualCameraInstance(mapCfg);

                //加载玩家实体
                LoadPlayerInstance(mapCfg.playerPath, mapCfg.playerBornPos, mapCfg.playerBornRote, new Vector3(0.8f, 0.8f, 0.8f), virtualCamera);

                //延迟激活第一批次怪物
                ActiveCurrentBatchMonsters();

                //切换BGM
                PlayBGAudioLst();

                SetPauseGame(false);

                cb?.Invoke();

                resSvc.UnloadUnusedAssets(Constants.ResourcePackgeName);
            });
        }

        private void PlayBGAudioLst()
        {
            List<string> auLst = new List<string> { Constants.BGHuangYe, Constants.BGBattle01 };
            audioSvc.StopBGMusic();
            audioSvc.PlayBGMusics(auLst, 3f, true);
        }

        //相关回调处理
        public virtual void AddEntityPlayerData()
        {
            GameStateEvent.MainInstance.CurrentEPlayer.OnValueChanged += delegate (EntityPlayer entity) { OnUpdateEntityPlayer(entity); };
        }

        private void OnUpdateEntityPlayer(EntityPlayer value)
        {
            PECommon.Log("玩家切换:" + value.EntityName);
        }

        //相关逻辑驱动
        private bool isPauseGameAI = false;
        public void SetPauseGame(bool stateAI)
        {
            isPauseGameAI = stateAI;
        }
        public bool GetPauseGame()
        {
            return isPauseGameAI;
        }

        private void Update()
        {
            CheckMonsterCount();

            RunMonsterTickLogic();
            RunPlayerTickLogic();
        }

        private void RunPlayerTickLogic()
        {

        }

        private void RunMonsterTickLogic()
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
                        var entitySelfPlayer = GameStateEvent.MainInstance.CurrentEPlayer.Value;
                        EndBattle(true, entitySelfPlayer.CurrentHP.Value);
                    }
                }
            }
        }

        //战斗结算处理
        public void EndBattle(bool isWin, int restHP)
        {
            SetPauseGame(true);

            var entitySelfPlayer = GameStateEvent.MainInstance.CurrentEPlayer.Value;
            entitySelfPlayer.StateIdle();
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
                    GameObject m = await resSvc.LoadGameObjectAsync(Constants.ResourcePackgeName, md.mCfg.resPath, md.mBornPos, Quaternion.Euler(md.mBornRote), new Vector3(0.8f, 0.8f, 0.8f), true, false, false);

                    m.name = "m" + md.mWave + "_" + md.mIndex;

                    EntityMonster em = new EntityMonster
                    {
                        battleMgr = this,
                        stateMgr = stateMgr, //将stateMgr注入逻辑实体类中
                        skillMgr = skillMgr,
                        VFXMgr = VFXMgr,
                    };
                    //设置初始属性
                    em.md = md;
                    em.EntityName = m.name;
                    em.AddEntityEventListener();
                    em.SetBattleProps(md.mCfg.bps);

                    MonsterController mc = m.GetComponent<MonsterController>();
                    mc.Init();
                    mc.MonsterMoveSpeed = md.mMoveSpeed;
                    mc.EnableDownSpeed = true;
                    em.SetCtrl(mc);

                    m.SetActive(false);
                    monsterDic.Add(m.name, em);
                    em.OnInitFSM();

                    //Boss血条特殊处理
                    if (md.mCfg.mType == cfg.MonsterType.Normal)
                    {
                        MessageBox.MainInstance.AddHpItemInfo(m.name, mc.hpRoot, em.CurrentHP.Value);
                    }
                    else if (md.mCfg.mType == cfg.MonsterType.Boss)
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
            return monsterDic.Values.ToList();
        }

        public List<EntityPlayer> GetEntityPlayers()
        {
            return playerDic.Values.ToList();
        }

        public void RmvMonster(string key)
        {
            if (monsterDic.TryGetValue(key, out EntityMonster entityMonster))
            {
                //移除数据
                monsterDic.Remove(key);
                //移除血条
                MessageBox.MainInstance.RmvHpItemInfo(key);
            }
        }

        public void RmvPlayer(string key)
        {
            EntityPlayer entityPlayer;
            if (playerDic.TryGetValue(key, out entityPlayer))
            {
                playerDic.Remove(key);
            }
        }

        #region 技能施放与角色控制
        public void SetSelfPlayerMoveDir(Vector2 dir)
        {
            var entitySelfPlayer = GameStateEvent.MainInstance.CurrentEPlayer.Value;
            //设置玩家移动
            //PECommon.Log(dir.ToString());

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
        private int[] comboArr = ConfigSvc.MainInstance.GetAllComboAction().ToArray(); //普攻连招技能id
        public int comboIndex = 0; //记录当前要存储的连招的id为第n个
        private void CalcNormalAtkCombo()
        {
            var entitySelfPlayer = GameStateEvent.MainInstance.CurrentEPlayer.Value;
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
            var entitySelfPlayer = GameStateEvent.MainInstance.CurrentEPlayer.Value;
            entitySelfPlayer.StateAttack(Constants.SkillID_Mar7th00_skill01);
        }
        private void PlayerReleaseSkill02()
        {
            //PECommon.Log("Click Skill02");
            var entitySelfPlayer = GameStateEvent.MainInstance.CurrentEPlayer.Value;
            entitySelfPlayer.StateAttack(Constants.SkillID_Mar7th00_skill02);
        }
        private void PlayerReleaseSkill03()
        {
            //PECommon.Log("Click Skill03");
            var entitySelfPlayer = GameStateEvent.MainInstance.CurrentEPlayer.Value;
            entitySelfPlayer.StateAttack(Constants.SkillID_Mar7th00_skill03);
        }
        public Vector2 GetDirInput()
        {
            return battleSys.GetDirInput();
        }

        #endregion

        private void OnDisable()
        {
            GameStateEvent.MainInstance.CurrentEPlayer.OnValueChanged -= delegate (EntityPlayer entity) { OnUpdateEntityPlayer(entity); };
        }

    }
}
