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
}