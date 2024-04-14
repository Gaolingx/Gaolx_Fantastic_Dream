//功能：死亡状态


public class StateDie : IState
{
    public void StateEnter(EntityBase entity, params object[] args)
    {
        entity.currentAniState = AniState.Die;
    }

    public void StateExit(EntityBase entity, params object[] args)
    {
        
    }

    public void StateProcess(EntityBase entity, params object[] args)
    {
        entity.SetAction(Constants.ActionDie);
        TimerSvc.Instance.AddTimeTask((int tid) =>
        {
            entity.controller.gameObject.SetActive(false);
        }, Constants.StateDieMonsterAnimTime);
    }
}

