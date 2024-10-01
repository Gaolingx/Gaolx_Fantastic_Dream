//功能：移动状态

using SangoUtils.Patchs_YooAsset.Utils;

namespace DarkGod.Main
{
    internal class StateMove : FSMLinkedStaterItemBase
    {
        internal override void OnEnter()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

            //当进入某个状态时，需要标记当前状态，当下一次进入该状态，判断是否已经处于目标状态，如果是则无需切换
            entity.currentAniState = AniState.Move;

            //PECommon.Log("StateMove:Enter State.");
        }

        internal override void OnExit()
        {
            //PECommon.Log("StateMove:Exit State.");
        }

        internal override void OnUpdate()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

            entity.SetAniBlend(Constants.BlendWalk);
            //PECommon.Log("StateMove:Process State.");
        }
    }
}
