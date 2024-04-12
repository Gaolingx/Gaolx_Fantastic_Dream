//功能：技能管理器


using System.Collections.Generic;
using UnityEngine;

public class SkillMgr : MonoBehaviour
{
    private ResSvc resSvc;
    private TimerSvc timerSvc;

    public void Init()
    {
        resSvc = ResSvc.Instance;
        timerSvc = TimerSvc.Instance;
        PECommon.Log("Init SkillMgr Done.");
    }

    public void SkillAttack(EntityBase entity, int skillID)
    {
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
                timerSvc.AddTimeTask((int tid) =>
                {
                    SkillAction(entity, skillData, index);
                }, sum);
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
        //获取场景里所有的怪物实体，遍历运算（计算满足条件的伤害）
        List<EntityMonster> monsterLst = caster.battleMgr.GetEntityMonsters();
        for (int i = 0; i < monsterLst.Count; i++)
        {
            EntityMonster target = monsterLst[i];
            //判断怪物与玩家的距离，角度
            if (InRange(caster.GetPlayerPos(), target.GetPos(), skillActionCfg.radius)
                && InAngle(caster.GetPlayerTrans(), target.GetPos(), skillActionCfg.angle))
            {
                //满足所有条件，计算伤害
                CalcDamage(caster, target, skillCfg, damage);
            }
        }
    }
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
            //计算闪避

            //计算属性加成（基础属性+技能加成）

            //计算暴击

            //计算穿甲

        }
        else if (skillCfg.dmgType == DamageType.AP)
        {
            //计算属性加成（基础属性+技能加成）

            //计算魔法抗性

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

        //设置技能动作
        entity.SetAction(skillData.aniAction);
        //设置特效
        entity.SetCFX(skillData.fx, skillData.skillTime);

        //设置技能位移
        CalcSkillMove(entity, skillData);

        //我们希望当技能施放的时候，移动不生效，直到技能结束
        entity.canControl = false;
        entity.PlayerCanControl();

        timerSvc.AddTimeTask((int tid) =>
        {
            entity.SetAniBlend(Constants.State_Mar7th00_Blend_Idle);
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
                timerSvc.AddTimeTask((int tid) => {
                    entity.SetSkillMoveState(true, speed);
                }, sum);
            }
            else
            {
                entity.SetSkillMoveState(true, speed);
            }

            sum += skillMoveCfg.moveTime; //同理，累加技能移动时间
            //延迟关闭SkillMove
            timerSvc.AddTimeTask((int tid) => {
                entity.SetSkillMoveState(false);
            }, sum);
        }
    }
}