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