//功能：攻击状态

public class StateAttack : IState
{
    public void StateEnter(EntityBase entity, params object[] args)
    {
        entity.currentAniState = AniState.Attack;
        PECommon.Log("StateAttack:Enter State.");
    }

    public void StateExit(EntityBase entity, params object[] args)
    {
        PECommon.Log("StateAttack:Exit State.");
    }

    public void StateProcess(EntityBase entity, params object[] args)
    {
        entity.SetAniBlend(Constants.State_Mar7th00_Blend_Skill_01);
        PECommon.Log("StateAttack:Process State.");
    }
}