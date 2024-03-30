//功能：逻辑实体基类

using StarterAssets;

public abstract class EntityBase
{
    //玩家当前状态
    public AniState currentAniState = AniState.None;

    public StateMgr stateMgr = null;
    public ThirdPersonController playerController = null;
    public StarterAssetsInputs playerInput = null;
    public Controller controller = null;

    //玩家状态切换
    public void PlayerStateMove()
    {
        stateMgr.ChangeStatus(this, AniState.Move, null);
    }
    public void PlayerStateIdle()
    {
        stateMgr.ChangeStatus(this, AniState.Idle, null);
    }
    public void PlayerStateAttack()
    {
        stateMgr.ChangeStatus(this, AniState.Attack, null);
    }

    public virtual void SetAniBlend(int blend)
    {
        if (playerController != null)
        {
            playerController.SetAniBlend(blend);
        }
    }
}