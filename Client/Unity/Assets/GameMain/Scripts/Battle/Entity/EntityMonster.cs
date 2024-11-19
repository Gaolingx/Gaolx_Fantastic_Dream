//功能：怪物逻辑实体

using UnityEngine;

namespace DarkGod.Main
{
    public class EntityMonster : EntityBase
    {
        public EntityMonster()
        {
            entityType = EntityType.Monster;
        }

        public MonsterData md;

        private float checkTime = Constants.MonsterCheckTime;
        private float checkCountTime = 0f;

        private float atkTime = Constants.MonsterAtkTime;
        private float atkCountTime = 0f;

        //考虑等级影响，覆盖父类SetBattleProps默认方法
        public override void SetBattleProps(BattleProps props)
        {
            int level = md.mLevel;

            //计算怪物属性，与等级相关
            BattleProps p = new BattleProps
            {
                hp = props.hp * level,
                ad = props.ad * level,
                ap = props.ap * level,
                addef = props.addef * level,
                apdef = props.apdef * level,
                dodge = props.dodge * level,
                pierce = props.pierce * level,
                critical = props.critical * level
            };

            Props = p;
            CurrentHP.Value = p.hp;
        }

        bool runAI = true;
        public override void TickAILogic()
        {
            if (!runAI)
            {
                return;
            }

            if (currentAniState == AniState.Idle || currentAniState == AniState.Move)
            {
                if (battleMgr.GetPauseGame())
                {
                    StateIdle();
                    return;
                }

                float delta = Time.deltaTime;
                checkCountTime += delta;
                if (checkCountTime < checkTime)
                {
                    return;
                }
                else
                {
                    //找到玩家，并攻击
                    //1.计算目标方向
                    Vector2 dir = CalcTargetDir();

                    //2.判断目标是否在攻击范围
                    if (!InAtkRange())
                    {
                        //2.1 不在：则设置移动方向，并进入移动状态
                        SetDir(dir);
                        StateMove();
                    }
                    else
                    {
                        //2.2 在：则停止移动，进行攻击
                        SetDir(Vector2.zero);
                        //3. 判断攻击间隔
                        atkCountTime += checkCountTime; //确保移动过程也在攻击间隔内
                        if (atkCountTime > atkTime)
                        {
                            //3.1 达到攻击时间，转向并攻击
                            SetAtkRotation(dir);
                            StateAttack(md.mCfg.skillID);
                            atkCountTime = 0;
                        }
                        else
                        {
                            //3.2 未达到攻击时间，进入Idle状态，等待
                            StateIdle();
                        }
                    }
                    checkCountTime = 0;
                    checkTime = PETools.RDInt(1, 5) * 1.0f / 10; //Random:0.1-0.5s
                }
            }
        }

        public override Vector2 CalcTargetDir()
        {
            EntityPlayer entityPlayer = eventMgr.CurrentEPlayer.Value;
            if (entityPlayer == null || entityPlayer.currentAniState == AniState.Die)
            {
                runAI = false;
                return Vector2.zero;
            }
            else
            {
                Vector3 target = entityPlayer.GetPos();
                Vector3 self = this.GetPos();
                return new Vector2(target.x - self.x, target.z - self.z).normalized;
            }
        }

        private bool InAtkRange()
        {
            EntityPlayer entityPlayer = eventMgr.CurrentEPlayer.Value;
            if (entityPlayer == null || entityPlayer.currentAniState == AniState.Die)
            {
                runAI = false;
                return false;
            }
            else
            {
                Vector3 target = entityPlayer.GetPos();
                Vector3 self = this.GetPos();
                target.y = 0; //不考虑y方向分量
                self.y = 0;
                float dis = Vector3.Distance(target, self);
                //将计算的距离与配置文件中比较，小于则处于攻击范围
                if (dis <= md.mCfg.atkDis)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override bool GetBreakState()
        {
            if (md.mCfg.isStop)
            {
                if (curtSkillCfg != null)
                {
                    return curtSkillCfg.isBreak;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                //全局不可被中断
                return false;
            }
        }

        public override void SetHPVal(int oldval, int newval)
        {
            if (md.mCfg.mType == cfg.MonsterType.Boss)
            {
                BattleSys.MainInstance.playerCtrlWnd.SetBossHPBarVal(oldval, newval, Props.hp);
            }
            else
            {
                base.SetHPVal(oldval, newval);
            }
        }
    }
}

