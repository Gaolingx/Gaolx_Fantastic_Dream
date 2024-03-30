//���ܣ��߼�ʵ�����

using StarterAssets;

public abstract class EntityBase
{
    //��ҵ�ǰ״̬
    public AniState currentAniState = AniState.None;

    public StateMgr stateMgr = null;
    public ThirdPersonController playerController = null;
    public StarterAssetsInputs playerInput = null;
    public Controller controller = null;

    //���״̬�л�
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

    public virtual void SetAction(int action)
    {
        if (playerController != null)
        {
            playerController.SetAniBlend(action);
        }
    }
}