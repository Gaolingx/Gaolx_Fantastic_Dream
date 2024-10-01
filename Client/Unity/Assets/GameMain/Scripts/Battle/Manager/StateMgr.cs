//功能：状态管理器

using SangoUtils.Patchs_YooAsset.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class StateMgr : MonoBehaviour
    {
        private FSMLinkedStater _fsmLinkedStater { get; set; }

        public void InitFSM(object owner)
        {
            _fsmLinkedStater = new FSMLinkedStater(owner);
            _fsmLinkedStater.AddStaterItem<StateBorn>();
            _fsmLinkedStater.AddStaterItem<StateIdle>();
            _fsmLinkedStater.AddStaterItem<StateMove>();
            _fsmLinkedStater.AddStaterItem<StateAttack>();
            _fsmLinkedStater.AddStaterItem<StateHit>();
            _fsmLinkedStater.AddStaterItem<StateDie>();

            PECommon.Log("Init StateMgr Done.");
        }

        public void ChangeStatus(EntityBase entity, AniState targetState, params object[] args)
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
            _fsmLinkedStater.UpdateCurrentStaterItem();
        }

        // 状态更新
        public void UpdateState()
        {
           
        }
    }
}