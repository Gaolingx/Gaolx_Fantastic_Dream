//功能：状态管理器

using SangoUtils.Patchs_YooAsset.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class StateMgr : MonoBehaviour
    {
        private Dictionary<int, FSMLinkedStater> fsmLinkedStaterDic = new Dictionary<int, FSMLinkedStater>();

        internal void InitFSM(EntityBase entity)
        {
            FSMLinkedStater _fsmLinkedStater = new FSMLinkedStater(entity);
            _fsmLinkedStater.AddStaterItem<StateBorn>();
            _fsmLinkedStater.AddStaterItem<StateIdle>();
            _fsmLinkedStater.AddStaterItem<StateMove>();
            _fsmLinkedStater.AddStaterItem<StateAttack>();
            _fsmLinkedStater.AddStaterItem<StateHit>();
            _fsmLinkedStater.AddStaterItem<StateDie>();

            fsmLinkedStaterDic.Add(entity.GetHashCode(), _fsmLinkedStater);

            PECommon.Log("Init StateMgr Done.");
        }

        internal void ChangeStatus(EntityBase entity, AniState targetState, params object[] args)
        {
            if (fsmLinkedStaterDic.TryGetValue(entity.GetHashCode(), out FSMLinkedStater _fsmLinkedStater))
            {
                _fsmLinkedStater.SetBlackboardValue("EntityBase", entity);
                if (entity.currentAniState == targetState)
                {
                    return;
                }

                switch (targetState)
                {
                    case AniState.Born:
                        _fsmLinkedStater.InvokeTargetStaterItem<StateBorn>(true);
                        break;
                    case AniState.Idle:
                        _fsmLinkedStater.InvokeTargetStaterItem<StateIdle>();
                        break;
                    case AniState.Move:
                        _fsmLinkedStater.InvokeTargetStaterItem<StateMove>();
                        break;
                    case AniState.Attack:
                        _fsmLinkedStater.SetBlackboardValue("StateAttackArgs", args);
                        _fsmLinkedStater.InvokeTargetStaterItem<StateAttack>();
                        break;
                    case AniState.Hit:
                        _fsmLinkedStater.InvokeTargetStaterItem<StateHit>();
                        break;
                    case AniState.Die:
                        _fsmLinkedStater.InvokeTargetStaterItem<StateDie>();
                        break;
                    default:
                        break;

                }
                UpdateCurrentState(entity);
            }
        }

        #region 状态更新
        internal void UpdateCurrentState(EntityBase entity)
        {
            if (fsmLinkedStaterDic.TryGetValue(entity.GetHashCode(), out FSMLinkedStater _fsmLinkedStater))
            {
                _fsmLinkedStater.UpdateCurrentStaterItem();
            }
        }

        internal void UpdateTargetState<T>(EntityBase entity) where T : FSMLinkedStaterItemBase
        {
            if (fsmLinkedStaterDic.TryGetValue(entity.GetHashCode(), out FSMLinkedStater _fsmLinkedStater))
            {
                _fsmLinkedStater.UpdateTargetStaterItem<T>();
            }
        }

        internal void UpdateAllState(EntityBase entity)
        {
            if (fsmLinkedStaterDic.TryGetValue(entity.GetHashCode(), out FSMLinkedStater _fsmLinkedStater))
            {
                _fsmLinkedStater.UpdateAllStaterItem();
            }
        }
        #endregion

        #region 状态删除
        internal void RemoveTargetState<T>(EntityBase entity) where T : FSMLinkedStaterItemBase
        {
            if (fsmLinkedStaterDic.TryGetValue(entity.GetHashCode(), out FSMLinkedStater _fsmLinkedStater))
            {
                _fsmLinkedStater.RemoveStaterItem<T>();
            }
        }
        #endregion
    }
}