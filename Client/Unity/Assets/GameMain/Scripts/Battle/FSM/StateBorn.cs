//功能：出生状态


namespace DarkGod.Main
{
    public class StateBorn : IState
    {
        public void StateEnter(EntityBase entity, params object[] args)
        {
            entity.currentAniState = AniState.Born;
        }

        public void StateExit(EntityBase entity, params object[] args)
        {

        }

        public void StateProcess(EntityBase entity, params object[] args)
        {
            //播放出生动画
            entity.SetAction(Constants.ActionBorn);
            TimerSvc.MainInstance.AddTimeTask((int tid) =>
            {
                entity.SetAction(Constants.ActionDefault);
            }, Constants.StateBornMonsterDurationTime); //应小于动画时长
        }
    }
}

