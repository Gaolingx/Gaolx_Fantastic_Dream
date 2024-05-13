//功能：状态管理器


using System.Collections.Generic;
using UnityEngine;

public class StateMgr : MonoBehaviour
{
    private Dictionary<AniState, IState> fsmDic = new Dictionary<AniState, IState>();

    public void Init()
    {
        //实例化所有状态
        fsmDic.Add(AniState.Born, new StateBorn());
        fsmDic.Add(AniState.Idle, new StateIdle());
        fsmDic.Add(AniState.Move, new StateMove());
        fsmDic.Add(AniState.Attack, new StateAttack());
        fsmDic.Add(AniState.Hit, new StateHit());
        fsmDic.Add(AniState.Die, new StateDie());

        PECommon.Log("Init StateMgr Done.");
    }

    public void ChangeStatus(EntityBase entity, AniState targetState, params object[] args)
    {
        if (entity.currentAniState == targetState)
        {
            return;
        }

        if (fsmDic.ContainsKey(targetState))
        {
            //从字典中取出当前实体中对应的状态，选择相应状态
            if (entity.currentAniState != AniState.None)
            {
                fsmDic[entity.currentAniState].StateExit(entity, args);
            }
            fsmDic[targetState].StateEnter(entity, args);
            fsmDic[targetState].StateProcess(entity, args);
        }
    }
}