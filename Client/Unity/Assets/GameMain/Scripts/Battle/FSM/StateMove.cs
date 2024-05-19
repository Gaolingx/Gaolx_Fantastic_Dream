namespace DarkGod.Main
{
//功能：移动状态

public class StateMove : IState
{
    public void StateEnter(EntityBase entity, params object[] args)
    {
        //当进入某个状态时，需要标记当前状态，当下一次进入该状态，判断是否已经处于目标状态，如果是则无需切换
        entity.currentAniState = AniState.Move;
        //PECommon.Log("StateMove:Enter State.");
    }

    public void StateExit(EntityBase entity, params object[] args)
    {
        //PECommon.Log("StateMove:Exit State.");
    }

    public void StateProcess(EntityBase entity, params object[] args)
    {
        entity.SetAniBlend(Constants.BlendWalk);
        //PECommon.Log("StateMove:Process State.");
    }
}


}