//功能：待机状态

public class StateIdle : IState
{
    public void EnterState(EntityBase entity)
    {
        entity.currentAniState = AniState.Idle;
        PECommon.Log("StateIdle:Enter State.");
    }

    public void ExitState(EntityBase entity)
    {
        PECommon.Log("StateIdle:Exit State.");
    }

    public void ProcessState(EntityBase entity)
    {
        PECommon.Log("StateIdle:Process State.");
    }
}