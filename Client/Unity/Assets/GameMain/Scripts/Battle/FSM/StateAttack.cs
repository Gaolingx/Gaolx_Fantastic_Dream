//功能：攻击状态


namespace DarkGod.Main
{
    public class StateAttack : IState
    {
        public void StateEnter(EntityBase entity, params object[] args)
        {
            entity.currentAniState = AniState.Attack;
            entity.curtSkillCfg = ResSvc.Instance.GetSkillCfg((int)args[0]);
            //PECommon.Log("StateAttack:Enter State.");
        }

        public void StateExit(EntityBase entity, params object[] args)
        {
            entity.ExitCurtSkill();
            //PECommon.Log("StateAttack:Exit State.");
        }

        public void StateProcess(EntityBase entity, params object[] args)
        {
            if (entity.entityType == EntityType.Player)
            {
                entity.CanRlsSkill = false;
            }

            entity.SkillAttack((int)args[0]);
            //PECommon.Log("StateAttack:Process State.");
        }
    }
}