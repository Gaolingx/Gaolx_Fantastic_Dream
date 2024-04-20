//功能：攻击状态


public class StateAttack : IState
{
    public void StateEnter(EntityBase entity, params object[] args)
    {
        entity.currentAniState = AniState.Attack;
        //PECommon.Log("StateAttack:Enter State.");
    }

    public void StateExit(EntityBase entity, params object[] args)
    {
        entity.canControl = true;
        entity.PlayerCanControl();
        entity.SetAction(Constants.ActionDefault);
        //PECommon.Log("StateAttack:Exit State.");
    }

    public void StateProcess(EntityBase entity, params object[] args)
    {
        entity.SkillAttack((int)args[0]);
        entity.SetInputBool((int)args[0]);
        //PECommon.Log("StateAttack:Process State.");
    }
}