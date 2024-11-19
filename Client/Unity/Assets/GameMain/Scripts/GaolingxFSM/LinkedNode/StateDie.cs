//功能：死亡状态


using SangoUtils.Patchs_YooAsset.Utils;

namespace DarkGod.Main
{
    internal class StateDie : FSMLinkedStaterItemBase
    {
        internal override void OnEnter()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

            entity.currentAniState = AniState.Die;

            entity.RmvSkillCB();
        }

        internal override void OnExit()
        {

        }

        internal override void OnUpdate()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

            entity.SetAction(Constants.ActionDie);
            if (entity.entityType == EntityType.Monster)
            {
                entity.GetCharacterController().enabled = false;
                TimerSvc.MainInstance.AddTimeTask((int tid) =>
                {
                    entity.SetActive(false);
                }, Constants.StateDieMonsterAnimTime);
            }
            entity.RmvEntityEventListener();
        }
    }
}

