//���ܣ�״̬�ӿ�

public interface IState
{
    void EnterState(EntityBase entity);

    void ProcessState(EntityBase entity);

    void ExitState(EntityBase entity);
}