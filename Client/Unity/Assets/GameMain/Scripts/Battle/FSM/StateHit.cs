//功能：受击状态

using UnityEngine;

namespace DarkGod.Main
{
    public class StateHit : IState
    {
        public void StateEnter(EntityBase entity, params object[] args)
        {
            entity.currentAniState = AniState.Hit;

            //根据tid删除定时回调，相应的伤害和移动将不生效
            DelTimeTaskByTid(entity);

            //清空连招数据
            ClearComboData(entity);
        }

        public void StateExit(EntityBase entity, params object[] args)
        {

        }

        public void StateProcess(EntityBase entity, params object[] args)
        {
            if (entity.entityType == EntityType.Player)
            {
                entity.CanRlsSkill = false;
            }

            //中断移动
            entity.SetDir(Vector2.zero);
            entity.SetAction(Constants.ActionHit);

            //受击音效
            if (entity.entityType == EntityType.Player)
            {
                entity.PlayHitAudio();
            }

            //恢复Idle状态
            TimerSvc.Instance.AddTimeTask((int tid) =>
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

        private void DelTimeTaskByTid(EntityBase entity)
        {
            for (int i = 0; i < entity.skMoveCBLst.Count; i++)
            {
                int tid = entity.skMoveCBLst[i];
                TimerSvc.Instance.DelTask(tid);
            }

            for (int i = 0; i < entity.skActionCBLst.Count; i++)
            {
                int tid = entity.skActionCBLst[i];
                TimerSvc.Instance.DelTask(tid);
            }
        }

        private void ClearComboData(EntityBase entity)
        {
            if (entity.nextSkillID != 0 || entity.comboQue.Count > 0)
            {
                entity.nextSkillID = 0;
                entity.comboQue.Clear();

                entity.battleMgr.lastAtkTime = 0;
                entity.battleMgr.comboIndex = 0;
            }
        }
    }
}

