//功能：状态接口

public interface IState
{
    void EnterState(EntityBase entity);

    void ProcessState(EntityBase entity);

    void ExitState(EntityBase entity);
}

public enum AniState
{
    None,
    Idle,
    Move,
}