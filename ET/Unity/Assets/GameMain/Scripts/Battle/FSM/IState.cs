//���ܣ�״̬�ӿ�

public interface IState
{
    void StateEnter(EntityBase entity, params object[] args);//�ɱ����

    void StateProcess(EntityBase entity, params object[] args);

    void StateExit(EntityBase entity, params object[] args);
}

public enum AniState
{
    None,
    Idle,
    Move,
}