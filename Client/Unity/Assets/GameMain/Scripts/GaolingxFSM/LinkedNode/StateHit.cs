﻿//功能：受击状态

using SangoUtils.Patchs_YooAsset.Utils;
using UnityEngine;

namespace DarkGod.Main
{
    internal class StateHit : FSMLinkedStaterItemBase
    {
        internal override void OnEnter()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

            entity.currentAniState = AniState.Hit;

            entity.RmvSkillCB();
        }

        internal override void OnExit()
        {

        }

        internal override void OnUpdate()
        {
            EntityBase entity = (EntityBase)_fsmLinkedStater.GetBlackboardValue("EntityBase");

            if (entity.entityType == EntityType.Player)
            {
                entity.CanRlsSkill.Value = false;
            }

            //中断移动
            entity.SetDir(Vector2.zero);
            entity.SetAction(Constants.ActionHit);

            //受击音效
            if (entity.entityType == EntityType.Player)
            {
                entity.PlayHitAudio(entity.GetTrans());
            }

            //恢复Idle状态
            TimerSvc.MainInstance.AddTimeTask((int tid) =>
            {
                entity.SetAction(Constants.ActionDefault);
                entity.StateIdle();
            }, (int)(GetHitAniLen(entity) * 1000));
        }

        //获取受击动画长度，单位：ms
        private float GetHitAniLen(EntityBase entity)
        {
            //获取entity上animator中受击动画长度(对应状态的motion长度)
            //实现思路：遍历动画状态机，遍历所有包含_hit名称的动画片段的受击动作，获取其Length（需规范命名，程序中做兼容性适配（如：大小写..））
            AnimationClip[] clips = entity.GetAniClips();
            for (int i = 0; i < clips.Length; i++)
            {
                string clipName = clips[i].name;
                if (clipName.Contains("hit") ||
                    clipName.Contains("Hit") ||
                    clipName.Contains("HIT"))
                {
                    //PECommon.Log("AniLength:" + clips[i].length);
                    return clips[i].length + Constants.HitAniLengthOffset;
                }
            }
            //保护值
            return 1;
        }

    }
}

