//功能：配置加载服务

using HuHu;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class ConfigSvc : Singleton<ConfigSvc>
    {
        protected override void Awake()
        {
            base.Awake();

            GameStateEvent.MainInstance.OnGameEnter += delegate { InitSvc(); };
        }

        public void InitSvc()
        {
            InitRDNameCfg();
            InitMonsterCfg();
            InitMapCfg();
            InitGuideCfg();
            InitStrongCfg();
            InitBuyCfg();
            InitTaskRewardCfg();
            InitNpcCfg();

            InitSkillCfg();
            InitSkillMoveCfg();
            InitSkillActionCfg();

            PECommon.Log("Init ConfigSvc...");
        }

        public void ResetSkillCfgs()
        {
            //清空字典，避免key冲突
            skillDic.Clear();
            InitSkillCfg();
            skillMoveDic.Clear();
            InitSkillMoveCfg();
            skillActionDic.Clear();
            InitSkillActionCfg();

            PECommon.Log("Reset Skill Cfgs Done.");
        }

        #region InitCfgs
        #region 随机名字
        private List<string> surnameLst = new List<string>();
        private List<string> manLst = new List<string>();
        private List<string> womanLst = new List<string>();

        private void InitRDNameCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var rdNameTable = tables.Tbrdname;

            foreach (var table in rdNameTable.DataMap.Values)
            {
                surnameLst.Add(table.Surname);
                manLst.Add(table.Man);
                womanLst.Add(table.Woman);
            }
        }

        public string GetRDNameCfg(bool man = true)
        {
            System.Random rd = new System.Random();
            string rdName = surnameLst[PETools.RDInt(0, surnameLst.Count - 1, rd)];
            if (man)
            {
                rdName += manLst[PETools.RDInt(0, manLst.Count - 1)];
            }
            else
            {
                rdName += womanLst[PETools.RDInt(0, womanLst.Count - 1)];
            }

            return rdName;
        }
        #endregion

        #region 地图
        private Dictionary<int, MapCfg> mapCfgDataDic = new Dictionary<int, MapCfg>();

        private void InitMapCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var mapCfgTable = tables.Tbmap;

            foreach (var table in mapCfgTable.DataMap.Values)
            {
                MapCfg mapCfg = new MapCfg()
                {
                    ID = table.ID,
                    monsterLst = new List<MonsterData>()
                };

                mapCfg.mapName = table.MapName;
                mapCfg.sceneName = table.SceneName;
                mapCfg.playerPath = table.PlayerPath;
                mapCfg.playerCamPath = table.PlayerCamPath;
                mapCfg.power = table.Power;
                mapCfg.mainCamPos = new Vector3(table.MainCamPos.X, table.MainCamPos.Y, table.MainCamPos.Z);
                mapCfg.mainCamRote = new Vector3(table.MainCamRote.X, table.MainCamRote.Y, table.MainCamRote.Z);
                mapCfg.playerBornPos = new Vector3(table.PlayerBornPos.X, table.PlayerBornPos.Y, table.PlayerBornPos.Z);
                mapCfg.playerBornRote = new Vector3(table.PlayerBornRote.X, table.PlayerBornRote.Y, table.PlayerBornRote.Z);
                mapCfg.coin = table.Coin;
                mapCfg.exp = table.Exp;
                mapCfg.crystal = table.Crystal;

                try
                {
                    string[] mLstArr = table.MonsterLst.Split('#');
                    for (int waveIndex = 0; waveIndex < mLstArr.Length; waveIndex++)
                    {
                        if (waveIndex == 0)
                        {
                            continue;
                        }
                        string[] tempArr = mLstArr[waveIndex].Split('|');
                        for (int j = 0; j < tempArr.Length; j++)
                        {
                            if (j == 0)
                            {
                                continue;
                            }
                            string[] arr = tempArr[j].Split(',');
                            MonsterData md = new MonsterData
                            {
                                ID = int.Parse(arr[0]),
                                mWave = waveIndex,
                                mIndex = j,
                                mCfg = GetMonsterCfg(int.Parse(arr[0])),
                                mBornPos = new Vector3(float.Parse(arr[1]), float.Parse(arr[2]), float.Parse(arr[3])),
                                mBornRote = new Vector3(0, float.Parse(arr[4]), 0),
                                mLevel = int.Parse(arr[5]),
                                mMoveSpeed = float.Parse(arr[6])
                            };
                            mapCfg.monsterLst.Add(md);
                        }
                    }
                }
                catch (Exception ex)
                {
                    PECommon.Log($"Error Load Config:{ex.Message}", PELogType.Error);
                }

                mapCfgDataDic.Add(table.ID, mapCfg);
            }
        }

        public MapCfg GetMapCfg(int id)
        {
            MapCfg data = null;
            if (mapCfgDataDic.TryGetValue(id, out data))
            {
                return data;
            }
            return null;
        }
        #endregion

        #region 自动引导配置
        private Dictionary<int, AutoGuideCfg> guideTaskDic = new Dictionary<int, AutoGuideCfg>();

        private void InitGuideCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var guideCfgTable = tables.Tbguide;

            foreach (var table in guideCfgTable.DataMap.Values)
            {
                AutoGuideCfg autoGuideCfg = new AutoGuideCfg
                {
                    ID = table.ID
                };

                autoGuideCfg.npcID = table.NpcID;
                autoGuideCfg.dilogArr = table.DilogArr;
                autoGuideCfg.actID = table.ActID;
                autoGuideCfg.coin = table.Coin;
                autoGuideCfg.exp = table.Exp;

                guideTaskDic.Add(table.ID, autoGuideCfg);
            }
        }

        public AutoGuideCfg GetAutoGuideCfg(int id)
        {
            AutoGuideCfg agc = null;
            if (guideTaskDic.TryGetValue(id, out agc))
            {
                return agc;
            }
            return null;
        }
        #endregion

        #region 强化升级配置
        private Dictionary<int, Dictionary<int, StrongCfg>> strongDic = new Dictionary<int, Dictionary<int, StrongCfg>>();

        private void InitStrongCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var strongCfgTable = tables.Tbstrong;

            foreach (var table in strongCfgTable.DataMap.Values)
            {
                StrongCfg sd = new StrongCfg
                {
                    ID = table.ID
                };

                sd.pos = table.Pos;
                sd.startlv = table.Starlv;
                sd.addhp = table.Addhp;
                sd.addhurt = table.Addhurt;
                sd.adddef = table.Adddef;
                sd.minlv = table.Minlv;
                sd.coin = table.Coin;
                sd.crystal = table.Crystal;

                try
                {
                    Dictionary<int, StrongCfg> dic = null;
                    //判断当前在该部位的字典是否存在
                    if (strongDic.TryGetValue(sd.pos, out dic))
                    {
                        //如果有则直接往字典增加数据项
                        dic.Add(sd.startlv, sd);
                    }
                    else
                    {
                        //如果没有，则需要先将该位置的字典new出来
                        dic = new Dictionary<int, StrongCfg>();
                        dic.Add(sd.startlv, sd);

                        //添加到strongDic中
                        strongDic.Add(sd.pos, dic);
                    }
                }
                catch (Exception ex)
                {
                    PECommon.Log($"Error Load Config:{ex.Message}", PELogType.Error);
                }
            }
        }

        //获取对应位置对应星级的属性
        public StrongCfg GetStrongCfg(int pos, int starlv)
        {
            StrongCfg sd = null;
            Dictionary<int, StrongCfg> dic = null;
            if (strongDic.TryGetValue(pos, out dic))
            {
                //判断字典中是否含有相应的星级
                if (dic.ContainsKey(starlv))
                {
                    sd = dic[starlv];
                }
            }
            return sd;
        }

        //获取某个星级包括前面所有星级在某个属性累加的和 
        public int GetPropAddValPreLv(int pos, int starlv, int type)
        {
            //获取对应位置所有的强化数据
            Dictionary<int, StrongCfg> posDic = null;
            int val = 0;
            if (strongDic.TryGetValue(pos, out posDic))
            {
                //根据星级和类型获取对应属性
                for (int i = 0; i < starlv; i++)
                {
                    StrongCfg sd;
                    if (posDic.TryGetValue(i, out sd))
                    {
                        //根据类型累加数值
                        switch (type)
                        {
                            case 1://hp
                                val += sd.addhp;
                                break;
                            case 2://hurt
                                val += sd.addhurt;
                                break;
                            case 3://def
                                val += sd.adddef;
                                break;
                        }
                    }
                }
            }
            return val;
        }
        #endregion

        #region 资源交易配置
        private Dictionary<int, BuyCfg> buyCfgDic = new Dictionary<int, BuyCfg>();

        private void InitBuyCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var buyCfgTable = tables.TbbuyCfg;

            foreach (var table in buyCfgTable.DataMap.Values)
            {
                BuyCfg buyCfg = new BuyCfg
                {
                    ID = table.ID
                };

                buyCfg.buyCostDiamondOnce = table.BuyCostDiamondOnce;
                buyCfg.amountEachPurchase = table.AmountEachPurchase;

                buyCfgDic.Add(table.ID, buyCfg);
            }
        }

        public BuyCfg GetBuyCfg(int id)
        {
            BuyCfg bc = null;
            if (buyCfgDic.TryGetValue(id, out bc))
            {
                return bc;
            }
            return null;
        }
        #endregion

        #region 任务奖励配置
        private Dictionary<int, TaskRewardCfg> taskRewardDic = new Dictionary<int, TaskRewardCfg>();

        private void InitTaskRewardCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var taskRewardCfgTable = tables.Tbtaskreward;

            foreach (var table in taskRewardCfgTable.DataMap.Values)
            {
                TaskRewardCfg trc = new TaskRewardCfg
                {
                    ID = table.ID
                };

                trc.taskName = table.TaskName;
                trc.count = table.Count;
                trc.exp = table.Exp;
                trc.coin = table.Coin;

                taskRewardDic.Add(table.ID, trc);
            }
        }

        public TaskRewardCfg GetTaskRewardCfg(int id)
        {
            TaskRewardCfg trc = null;
            if (taskRewardDic.TryGetValue(id, out trc))
            {
                return trc;
            }
            return null;
        }
        #endregion

        #region 全局NPC配置
        private Dictionary<int, NpcData> npcDic = new Dictionary<int, NpcData>();

        private void InitNpcCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var npcCfgTable = tables.TbnpcCfg;

            foreach (var table in npcCfgTable.DataMap.Values)
            {
                NpcData nd = new NpcData
                {
                    ID = table.ID
                };

                nd.npcName = table.NPCName;
                nd.npcResPath = table.NPCResPath;
                nd.NPC_Transform_Position = new Vector3(table.NPCTransformPosition.X, table.NPCTransformPosition.Y, table.NPCTransformPosition.Z);
                nd.NPC_Transform_Rotation = new Vector3(table.NPCTransformRotation.X, table.NPCTransformRotation.Y, table.NPCTransformRotation.Z);
                nd.NPC_Transform_Scale = new Vector3(table.NPCTransformScale.X, table.NPCTransformScale.Y, table.NPCTransformScale.Z);

                npcDic.Add(table.ID, nd);
            }
        }

        public NpcData GetNpcCfg(int id)
        {
            NpcData nd = null;
            if (npcDic.TryGetValue(id, out nd))
            {
                return nd;
            }
            return null;
        }
        #endregion

        #region 技能配置
        private Dictionary<int, SkillCfg> skillDic = new Dictionary<int, SkillCfg>();

        private void InitSkillCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var skCommonCfgTable = tables.Tbskill;

            foreach (var table in skCommonCfgTable.DataMap.Values)
            {
                SkillCfg sc = new SkillCfg()
                {
                    ID = table.ID,
                    skillMoveLst = new List<int>(),
                    skillActionLst = new List<int>(),
                    skillDamageLst = new List<int>()
                };

                sc.skillName = table.SkillName;
                sc.cdTime = table.CdTime;
                sc.skillTime = table.SkillTime;
                sc.aniAction = table.AniAction;
                sc.fx = table.Fx;
                sc.isCombo = table.IsCombo;
                sc.isCollide = table.IsCollide;
                sc.isBreak = table.IsBreak;
                sc.dmgType = table.DmgType;

                try
                {
                    string[] skMoveArr = table.SkillMoveLst.Split('|');
                    for (int j = 0; j < skMoveArr.Length; j++)
                    {
                        if (skMoveArr[j] != "")
                        {
                            sc.skillMoveLst.Add(int.Parse(skMoveArr[j]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    PECommon.Log($"Error Load Config:{ex.Message}", PELogType.Error);
                }

                try
                {
                    string[] skActionArr = table.SkillActionLst.Split('|');
                    for (int j = 0; j < skActionArr.Length; j++)
                    {
                        if (skActionArr[j] != "")
                        {
                            sc.skillActionLst.Add(int.Parse(skActionArr[j]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    PECommon.Log($"Error Load Config:{ex.Message}", PELogType.Error);
                }

                try
                {
                    string[] skDamageArr = table.SkillDamageLst.Split('|');
                    for (int j = 0; j < skDamageArr.Length; j++)
                    {
                        if (skDamageArr[j] != "")
                        {
                            sc.skillDamageLst.Add(int.Parse(skDamageArr[j]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    PECommon.Log($"Error Load Config:{ex.Message}", PELogType.Error);
                }

                skillDic.Add(table.ID, sc);
            }
        }

        public SkillCfg GetSkillCfg(int id)
        {
            SkillCfg sc = null;
            if (skillDic.TryGetValue(id, out sc))
            {
                return sc;
            }
            return null;
        }

        public List<int> GetAllComboAction()
        {
            List<int> listCombo = new List<int>();
            foreach (SkillCfg sc in skillDic.Values)
            {
                if (sc.isCombo)
                {
                    listCombo.Add(sc.ID);
                }
            }
            return listCombo;
        }
        #endregion

        #region 技能位移配置
        private Dictionary<int, SkillMoveCfg> skillMoveDic = new Dictionary<int, SkillMoveCfg>();

        private void InitSkillMoveCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var skMoveTable = tables.Tbskillmove;

            foreach (var table in skMoveTable.DataMap.Values)
            {
                SkillMoveCfg smc = new SkillMoveCfg
                {
                    ID = table.ID
                };

                smc.delayTime = table.DelayTime;
                smc.moveTime = table.MoveTime;
                smc.moveDis = table.MoveDis;

                skillMoveDic.Add(table.ID, smc);
            }
        }

        public SkillMoveCfg GetSkillMoveCfg(int id)
        {
            SkillMoveCfg smc = null;
            if (skillMoveDic.TryGetValue(id, out smc))
            {
                return smc;
            }
            return null;
        }
        #endregion

        #region 技能Action配置
        private Dictionary<int, SkillActionCfg> skillActionDic = new Dictionary<int, SkillActionCfg>();

        private void InitSkillActionCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var skActionTable = tables.Tbskillaction;

            foreach (var table in skActionTable.DataMap.Values)
            {
                SkillActionCfg sac = new SkillActionCfg
                {
                    ID = table.ID
                };

                sac.delayTime = table.DelayTime;
                sac.radius = table.Radius;
                sac.angle = table.Angle;

                skillActionDic.Add(table.ID, sac);
            }
        }

        public SkillActionCfg GetSkillActionCfg(int id)
        {
            SkillActionCfg sac = null;
            if (skillActionDic.TryGetValue(id, out sac))
            {
                return sac;
            }
            return null;
        }
        #endregion

        #region 怪物属性配置
        private Dictionary<int, MonsterCfg> monsterCfgDataDic = new Dictionary<int, MonsterCfg>();

        private void InitMonsterCfg()
        {
            var tables = new cfg.Tables(LubanHelper.LoadByteBufJson);
            var monsterTable = tables.Tbmonster;

            foreach (var table in monsterTable.DataMap.Values)
            {
                MonsterCfg mc = new MonsterCfg
                {
                    ID = table.ID,
                    bps = new BattleProps()
                };

                mc.mName = table.MName;
                mc.mType = table.MType;
                mc.resPath = table.ResPath;
                mc.isStop = table.IsStop;
                mc.skillID = table.SkillID;
                mc.atkDis = table.AtkDis;
                mc.bps.hp = table.Hp;
                mc.bps.ad = table.Ad;
                mc.bps.ap = table.Ap;
                mc.bps.addef = table.Addef;
                mc.bps.apdef = table.Apdef;
                mc.bps.dodge = table.Dodge;
                mc.bps.pierce = table.Pierce;
                mc.bps.critical = table.Critical;

                monsterCfgDataDic.Add(table.ID, mc);
            }
        }

        public MonsterCfg GetMonsterCfg(int id)
        {
            MonsterCfg data = null;
            if (monsterCfgDataDic.TryGetValue(id, out data))
            {
                return data;
            }
            return null;
        }
        #endregion

        #endregion

        private void OnDisable()
        {
            GameStateEvent.MainInstance.OnGameEnter -= delegate { InitSvc(); };
        }
    }
}
