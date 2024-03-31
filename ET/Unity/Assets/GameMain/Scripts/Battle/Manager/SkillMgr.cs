//功能：技能管理器


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

        timerSvc.AddTimeTask((int tid) =>
        {
            entity.SetAniBlend(Constants.State_Mar7th00_Blend_Idle);
            //不要直接在这里设置action，要考虑技能被打断的情况，因此我们需要在FSM中设置
        }, skillData.skillTime);
    }
}