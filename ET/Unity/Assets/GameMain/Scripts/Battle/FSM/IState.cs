//功能：状态接口

public interface IState
{
    void StateEnter(EntityBase entity, params object[] args);//可变参数

    void StateProcess(EntityBase entity, params object[] args);

    void StateExit(EntityBase entity, params object[] args);
}

public enum AniState
{
    None,
    Idle,
    Move,
}