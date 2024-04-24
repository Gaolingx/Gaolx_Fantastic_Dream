//功能：待机状态

using UnityEngine;

public class StateIdle : IState
{
    public void StateEnter(EntityBase entity, params object[] args)
    {
        entity.currentAniState = AniState.Idle;
        entity.SetDir(Vector2.zero);
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
            if (entity.GetDirInput() != Vector2.zero)
            {
                //玩家实体，且在UI中有操作
                entity.StateMove();
                entity.SetAniBlend(Constants.State_Mar7th00_Blend_Move);
                entity.SetDir(entity.GetDirInput());
            }
            else
            {
                //怪物实体，进入Idle状态
                entity.SetAniBlend(Constants.State_Mar7th00_Blend_Idle);
            }
            //PECommon.Log("StateIdle:Process State.");
        }
    }
}