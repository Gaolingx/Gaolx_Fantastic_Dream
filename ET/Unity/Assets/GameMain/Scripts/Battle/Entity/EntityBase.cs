//功能：逻辑实体基类

public class EntityBase
{
    //玩家当前状态
    public AniState currentAniState = AniState.None;

    public StateMgr stateMgr = null;

    //玩家状态切换
    public void PlayerStateMove()
    {
        stateMgr.ChangeStatus(this, AniState.Move);
    }

    public void PlayerStateIdle()
    {
        stateMgr.ChangeStatus(this, AniState.Idle);
    }
}