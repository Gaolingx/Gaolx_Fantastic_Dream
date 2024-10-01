//功能：出生状态


using SangoUtils.Patchs_YooAsset.Utils;

namespace DarkGod.Main
{
    internal class StateBorn : FSMLinkedStaterItemBase
    {
        internal override void OnEnter()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

            entity.currentAniState = AniState.Born;
        }

        internal override void OnExit()
        {

        }

        internal override void OnUpdate()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

            //播放出生动画
            entity.SetAction(Constants.ActionBorn);
            TimerSvc.MainInstance.AddTimeTask((int tid) =>
            {
                entity.SetAction(Constants.ActionDefault);
            }, Constants.StateBornMonsterDurationTime); //应小于动画时长
        }
    }
}

