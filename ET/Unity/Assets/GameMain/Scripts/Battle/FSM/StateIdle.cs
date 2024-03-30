//功能：待机状态

public class StateIdle : IState
{
    public void StateEnter(EntityBase entity, params object[] args)
    {
        entity.currentAniState = AniState.Idle;
        PECommon.Log("StateIdle:Enter State.");
    }

    public void StateExit(EntityBase entity, params object[] args)
    {
        PECommon.Log("StateIdle:Exit State.");
    }

    public void StateProcess(EntityBase entity, params object[] args)
    {
        PECommon.Log("StateIdle:Process State.");
        entity.SetAniBlend(Constants.State_Mar7th00_Blend_Idle);
    }
}