//功能：技能管理器


using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class SkillMgr : MonoBehaviour
    {
        private ResSvc resSvc;
        private TimerSvc timerSvc;
        private AudioSvc audioSvc;

        public bool isSetAtkRotation = false;

        public void Init()
        {
            resSvc = ResSvc.MainInstance;
            timerSvc = TimerSvc.MainInstance;
            audioSvc = AudioSvc.MainInstance;
            PECommon.Log("Init SkillMgr Done.");
        }

        public void SkillAttack(EntityBase entity, int skillID)
        {
            //清理技能回调列表
            entity.ClearActionCBLst();

            //技能伤害运算
            AttackDamage(entity, skillID);
            //技能效果表现
            AttackEffect(entity, skillID);
        }

        public void AttackDamage(EntityBase entity, int skillID)
        {
            SkillCfg skillData = resSvc.GetSkillCfg(skillID);
            //获取ActionList
            List<int> actonLst = skillData.skillActionLst;
            int sum = 0;
            for (int i = 0; i < actonLst.Count; i++)
            {
                SkillActionCfg skillActionCfg = resSvc.GetSkillActionCfg(actonLst[i]);
                sum += skillActionCfg.delayTime;
                int index = i; //action索引号
                if (sum > 0)
                {
                    //延时伤害计算
                    int actid = timerSvc.AddTimeTask((int tid) =>
                    {
                        if (entity != null)
                        {
                            SkillAction(entity, skillData, index);
                            entity.RmvActionCB(tid);
                        }
                    }, sum);
                    entity.skActionCBLst.Add(actid);
                }
                else
                {
                    //瞬时技能
                    SkillAction(entity, skillData, index);
                }
            }
        }

        public void SkillAction(EntityBase caster, SkillCfg skillCfg, int index)
        {
            SkillActionCfg skillActionCfg = resSvc.GetSkillActionCfg(skillCfg.skillActionLst[index]);

            int damage = skillCfg.skillDamageLst[index];
            if (caster.entityType == EntityType.Monster)
            {
                //怪物攻击玩家

                EntityPlayer epTarget = caster.battleMgr.EntityPlayer.Value;
                if (epTarget == null)
                {
                    return;
                }
                //判断距离，判断角度
                if (InRange(caster.GetPos(), epTarget.GetPos(), skillActionCfg.radius)
                    && InAngle(caster.GetTrans(), epTarget.GetPos(), skillActionCfg.angle))
                {
                    //满足所有条件，计算伤害
                    CalcDamage(caster, epTarget, skillCfg, damage);
                }
            }
            else if (caster.entityType == EntityType.Player)
            {
                //玩家攻击怪物

                //获取场景里所有的怪物实体，遍历运算（计算满足条件的伤害）
                List<EntityMonster> monsterLst = caster.battleMgr.GetEntityMonsters();
                for (int i = 0; i < monsterLst.Count; i++)
                {
                    EntityMonster emTarget = monsterLst[i];
                    //判断距离，判断角度
                    if (InRange(caster.GetPos(), emTarget.GetPos(), skillActionCfg.radius)
                        && InAngle(caster.GetTrans(), emTarget.GetPos(), skillActionCfg.angle))
                    {
                        //满足所有条件，计算伤害
                        CalcDamage(caster, emTarget, skillCfg, damage);
                    }
                }
            }
        }

        System.Random rd = new System.Random();
        /// <summary>
        /// 根据不同类型计算伤害
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <param name="target">目标实体</param>
        /// <param name="skillCfg">技能配置</param>
        /// <param name="damage">技能加成</param>
        private void CalcDamage(EntityBase caster, EntityBase target, SkillCfg skillCfg, int damage)
        {
            int dmgSum = damage;
            //根据不同属性计算伤害
            if (skillCfg.dmgType == DamageType.AD)
            {
                //计算闪避（优先计算）
                int dodgeNum = PETools.RDInt(1, 100, rd);
                if (dodgeNum <= target.Props.dodge)
                {
                    //UI显示闪避
                    //PECommon.Log("闪避Rate:" + dodgeNum + "/" + target.Props.dodge);
                    target.SetDodge();
                    return;
                }
                //计算属性加成（基础属性+技能加成）
                dmgSum += caster.Props.ad;

                //计算暴击
                int criticalNum = PETools.RDInt(1, 100, rd); //暴击概率
                if (criticalNum <= caster.Props.critical)
                {
                    float criticalRate = 1 + (PETools.RDInt(1, 100, rd) / 100.0f); //暴击倍率
                    dmgSum = (int)criticalRate * dmgSum; //暴击伤害
                    //PECommon.Log("暴击Rate:" + criticalNum + "/" + caster.Props.critical);
                    target.SetCritical(dmgSum);
                }

                //计算穿甲
                int addef = (int)((1 - caster.Props.pierce / 100.0f) * target.Props.addef); //计算护甲
                dmgSum -= addef; //减去护甲抵消伤害
            }
            else if (skillCfg.dmgType == DamageType.AP)
            {
                //计算属性加成（基础属性+技能加成）
                dmgSum += caster.Props.ap;
                //计算魔法抗性
                dmgSum -= target.Props.apdef;
            }
            else
            {
                PECommon.Log("DamageType dose not exist. DamageType:" + skillCfg.dmgType, PELogType.Error);
            }

            //最终伤害
            if (dmgSum < 0)
            {
                dmgSum = 0;
                return;
            }
            target.SetHurt(dmgSum);

            //目标应用伤害
            if (target.currentHP.Value <= dmgSum)
            {
                TargetDie(target);
            }
            else
            {
                target.currentHP.Value -= dmgSum;
                if (target.entityState == EntityState.None && target.GetBreakState()) //技能可被中断
                {
                    target.StateHit();
                }

            }
        }

        private void TargetDie(EntityBase target)
        {
            target.currentHP.Value = 0;
            //目标死亡
            target.StateDie();
            if (target.entityType == EntityType.Monster)
            {
                target.battleMgr.RmvMonster(target.Name);
            }
            else if (target.entityType == EntityType.Player)
            {
                //战斗失败
                target.battleMgr.EndBattle(false, 0);
            }

        }

        /// <summary>
        /// 玩家打怪物——范围判定
        /// </summary>
        /// <param name="from">起始位置</param>
        /// <param name="to">目标位置</param>
        /// <param name="range">两者的范围</param>
        /// <returns>是否在距离范围中</returns>
        private bool InRange(Vector3 from, Vector3 to, float range)
        {
            float dis = Vector3.Distance(from, to);
            if (dis <= range)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 玩家打怪物——角度判定
        /// </summary>
        /// <param name="trans">施法主体的Transform</param>
        /// <param name="to">目标位置</param>
        /// <param name="angle">角度的范围</param>
        /// <returns>是否在角度范围中</returns>
        private bool InAngle(Transform trans, Vector3 to, float angle)
        {
            if (angle == 360)
            {
                return true;
            }
            else
            {
                Vector3 start = trans.forward; //玩家朝向向量
                Vector3 dir = (to - trans.position).normalized; //目标朝向

                float ang = Vector3.Angle(start, dir); //夹角，无符号

                if (ang <= angle / 2)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 技能效果表现
        /// </summary>
        public void AttackEffect(EntityBase entity, int skillID)
        {
            SkillCfg skillData = resSvc.GetSkillCfg(skillID);

            //考虑碰撞
            if (!skillData.isCollide)
            {
                //忽略刚体碰撞（玩家与怪物之间，不包括环境、需要指定layer）
                Physics.IgnoreLayerCollision(Constants.PlayerCollideLayer, Constants.MonsterCollideLayer);
                timerSvc.AddTimeTask((int tid) =>
                {
                    Physics.IgnoreLayerCollision(Constants.PlayerCollideLayer, Constants.MonsterCollideLayer, false);
                }, skillData.skillTime);
            }

            //仅玩家
            if (entity.entityType == EntityType.Player)
            {
                //设置技能方向
                if (entity.GetDirInput() == Vector2.zero)
                {
                    //搜索最近的怪物
                    Vector2 dir = entity.CalcTargetDir();
                    if (dir != Vector2.zero)
                    {
                        entity.SetAtkRotation(dir);
                    }
                }
                else
                {
                    //将方向设置到实体类
                    if (isSetAtkRotation)
                    {
                        entity.SetAtkRotation(entity.GetDirInput());
                    }
                }
            }

            //设置技能动作
            entity.SetAction(skillData.aniAction);
            //设置特效
            entity.SetCFX(skillData.fx, skillData.skillTime, audioSvc.CharacterFxAudioVolumeValue);

            //设置技能位移
            CalcSkillMove(entity, skillData);

            //我们希望当技能施放的时候，移动不生效，直到技能结束
            entity.PlayerCanControl(false);
            entity.SetDir(Vector2.zero);

            //霸体状态
            if (!skillData.isBreak)
            {
                entity.entityState = EntityState.BatiState;
            }

            entity.skEndCB = timerSvc.AddTimeTask((int tid) =>
            {
                entity.StateIdle();
                //不要直接在这里设置action，要考虑技能被打断的情况，因此我们需要在FSM中设置
            }, skillData.skillTime);
        }
        private void CalcSkillMove(EntityBase entity, SkillCfg skillData)
        {
            List<int> skillMoveLst = skillData.skillMoveLst;
            int sum = 0;
            for (int i = 0; i < skillMoveLst.Count; i++)
            {
                SkillMoveCfg skillMoveCfg = resSvc.GetSkillMoveCfg(skillData.skillMoveLst[i]);
                float speed = skillMoveCfg.moveDis / (skillMoveCfg.moveTime / 1000f);
                sum += skillMoveCfg.delayTime; //多段位移技能要累加延迟时间
                if (sum > 0)
                {
                    //延迟执行SkillMove
                    int moveid = timerSvc.AddTimeTask((int tid) =>
                    {
                        entity.SetSkillMoveState(true, speed);
                        entity.RmvMoveCB(tid);
                    }, sum);
                    entity.skMoveCBLst.Add(moveid);
                }
                else
                {
                    entity.SetSkillMoveState(true, speed);
                }

                sum += skillMoveCfg.moveTime; //同理，累加技能移动时间
                                              //延迟关闭SkillMove
                int stopid = timerSvc.AddTimeTask((int tid) =>
                {
                    entity.SetSkillMoveState(false);
                    entity.RmvMoveCB(tid);
                }, sum);
                entity.skMoveCBLst.Add(stopid);
            }
        }
    }
}