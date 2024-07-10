//功能：玩家逻辑实体


using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class EntityPlayer : EntityBase
    {
        public EntityPlayer()
        {
            entityType = EntityType.Player;
        }

        public override Vector2 GetDirInput()
        {
            return battleMgr.GetDirInput();
        }

        public override Vector2 CalcTargetDir()
        {
            EntityMonster monster = FindClosedTarget();
            if (monster != null)
            {
                Vector3 target = monster.GetPos();
                Vector3 self = GetPos();
                Vector2 dir = new Vector2(target.x - self.x, target.z - self.z); //方向向量
                return dir.normalized;
            }
            else
            {
                return Vector2.zero;
            }
        }

        private EntityMonster FindClosedTarget()
        {
            //获取场景中所有怪物
            List<EntityMonster> lst = battleMgr.GetEntityMonsters();
            if (lst == null || lst.Count == 0)
            {
                return null;
            }

            Vector3 self = GetPos();
            EntityMonster targetMonster = null;
            float dis = 0;

            //遍历列表所有怪物，计算他们距离，排序，直到找到最近的为止
            for (int i = 0; i < lst.Count; i++)
            {
                Vector3 target = lst[i].GetPos();
                if (i == 0)
                {
                    dis = Vector3.Distance(self, target); //玩家自己与第一个怪物的距离
                    targetMonster = lst[0];
                }
                else
                {
                    float calcDis = Vector3.Distance(self, target);
                    if (dis > calcDis)
                    {
                        dis = calcDis; //找到距离更近的怪物
                        targetMonster = lst[i];
                    }
                }
            }
            return targetMonster;
        }

        public override void SetHPVal(int oldval, int newval)
        {
            if (playerController != null)
            {
                BattleSys.Instance.playerCtrlWnd.SetSelfHPBarVal(newval);
            }
        }

        public override void SetDodge()
        {
            if (playerController != null)
            {
                GameRoot.MainInstance.dynamicWnd.SetSelfDodge();
            }
        }

        public override void PlayHitAudio()
        {
            AudioSvc.MainInstance.PlayHit();
        }
    }
}

