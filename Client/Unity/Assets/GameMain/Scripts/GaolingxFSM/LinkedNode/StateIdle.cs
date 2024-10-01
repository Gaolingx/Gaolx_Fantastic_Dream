//功能：待机状态

using SangoUtils.Patchs_YooAsset.Utils;
using UnityEngine;

namespace DarkGod.Main
{
    internal class StateIdle : FSMLinkedStaterItemBase
    {
        internal override void OnEnter()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

            entity.currentAniState = AniState.Idle;
            entity.SetDir(Vector2.zero);
            entity.skEndCB = -1;

            //PECommon.Log("StateIdle:Enter State.");
        }

        internal override void OnExit()
        {
            //PECommon.Log("StateIdle:Exit State.");
        }

        internal override void OnUpdate()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

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