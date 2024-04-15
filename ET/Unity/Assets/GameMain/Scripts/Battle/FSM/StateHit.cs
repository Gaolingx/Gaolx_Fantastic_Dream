//功能：受击状态

using UnityEngine;

public class StateHit : IState
{
    public void StateEnter(EntityBase entity, params object[] args)
    {
        entity.currentAniState = AniState.Hit;
    }

    public void StateExit(EntityBase entity, params object[] args)
    {
        
    }

    public void StateProcess(EntityBase entity, params object[] args)
    {
        //中断移动
        entity.SetDir(Vector2.zero);
        entity.SetAction(Constants.ActionHit);

        //TODO 恢复Idle状态
    }
}

