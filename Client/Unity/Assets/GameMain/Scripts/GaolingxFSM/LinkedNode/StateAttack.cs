//功能：攻击状态


using SangoUtils.Patchs_YooAsset.Utils;

namespace DarkGod.Main
{
    internal class StateAttack : FSMLinkedStaterItemBase
    {
        internal override void OnEnter()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");
            object[] args = (object[])_fsmLinkedStater.GetBlackboardValue("StateAttackArgs");

            entity.currentAniState = AniState.Attack;
            entity.curtSkillCfg = ConfigSvc.MainInstance.GetSkillCfg((int)args[0]);
            //PECommon.Log("StateAttack:Enter State.");
        }

        internal override void OnExit()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

            entity.ExitCurtSkill();
            //PECommon.Log("StateAttack:Exit State.");
        }

        internal override void OnUpdate()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");
            object[] args = (object[])_fsmLinkedStater.GetBlackboardValue("StateAttackArgs");

            if (entity.entityType == EntityType.Player)
            {
                entity.CanRlsSkill = false;
            }

            entity.SkillAttack((int)args[0]);
            //PECommon.Log("StateAttack:Process State.");
        }
    }
}