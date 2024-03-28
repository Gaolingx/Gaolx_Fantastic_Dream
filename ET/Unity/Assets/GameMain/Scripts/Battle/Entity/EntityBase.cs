//���ܣ��߼�ʵ�����

using StarterAssets;

public class EntityBase
{
    //��ҵ�ǰ״̬
    public AniState currentAniState = AniState.None;

    public StateMgr stateMgr = null;
    public ThirdPersonController PlayerController = null;
    public Controller controller = null;

    //���״̬�л�
    public void PlayerStateMove()
    {
        stateMgr.ChangeStatus(this, AniState.Move);
    }

    public void PlayerStateIdle()
    {
        stateMgr.ChangeStatus(this, AniState.Idle);
    }
}