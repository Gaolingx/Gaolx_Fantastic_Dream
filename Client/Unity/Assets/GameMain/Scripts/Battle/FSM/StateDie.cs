//功能：死亡状态


namespace DarkGod.Main
{
    public class StateDie : IState
    {
        public void StateEnter(EntityBase entity, params object[] args)
        {
            entity.currentAniState = AniState.Die;

            entity.RmvSkillCB();
        }

        public void StateExit(EntityBase entity, params object[] args)
        {

        }

        public void StateProcess(EntityBase entity, params object[] args)
        {
            entity.SetAction(Constants.ActionDie);
            if (entity.entityType == EntityType.Monster)
            {
                entity.GetCharacterController().enabled = false;
                TimerSvc.Instance.AddTimeTask((int tid) =>
                {
                    entity.SetActive(false);
                }, Constants.StateDieMonsterAnimTime);
            }
        }
    }
}

