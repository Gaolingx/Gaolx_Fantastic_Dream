//功能：移动状态

public class StateMove : IState
{
    public void EnterState(EntityBase entity)
    {
        //当进入某个状态时，需要标记当前状态，当下一次进入该状态，判断是否已经处于目标状态，如果是则无需切换
        entity.currentAniState = AniState.Move;
        PECommon.Log("StateMove:Enter State.");
    }

    public void ExitState(EntityBase entity)
    {
        PECommon.Log("StateMove:Exit State.");
    }

    public void ProcessState(EntityBase entity)
    {
        PECommon.Log("StateMove:Process State.");
    }
}

