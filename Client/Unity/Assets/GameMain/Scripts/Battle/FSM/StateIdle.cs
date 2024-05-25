//功能：待机状态

using UnityEngine;

namespace DarkGod.Main
{
    public class StateIdle : IState
    {
        public void StateEnter(EntityBase entity, params object[] args)
        {
            entity.currentAniState = AniState.Idle;
            entity.SetDir(Vector2.zero);
            entity.skEndCB = -1;
            //PECommon.Log("StateIdle:Enter State.");
        }

        public void StateExit(EntityBase entity, params object[] args)
        {
            //PECommon.Log("StateIdle:Exit State.");
        }

        public void StateProcess(EntityBase entity, params object[] args)
        {
            //连招判定
            if (entity.nextSkillID != 0)
            {
                entity.StateAttack(entity.nextSkillID);
            }
            else
            {
                if (entity.entityType == EntityType.Player)
                {
                    entity.CanRlsSkill = true;
                }

                if (entity.GetDirInput() != Vector2.zero)
                {
                    //玩家实体，且在UI中有操作
                    entity.StateMove();
                    entity.SetDir(entity.GetDirInput());
                }
                else
                {
                    //怪物实体，进入Idle状态
                    entity.SetAniBlend(Constants.BlendIdle);
                }
                //PECommon.Log("StateIdle:Process State.");
            }
        }
    }
}