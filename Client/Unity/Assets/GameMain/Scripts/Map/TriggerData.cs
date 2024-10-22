//功能：地图触发数据类

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    [RequireComponent(typeof(BoxCollider))]
    public class TriggerData : MonoBehaviour
    {
        public MapMgr mapMgr;
        public int triggerWave;

        public void OnTriggerExit(Collider other)
        {
            //过滤玩家标签
            if (other.gameObject.CompareTag(Constants.CharPlayerWithTag))
            {
                //调用地图管理器生成下一批怪物
                if (mapMgr != null)
                {
                    mapMgr.TriggerMonsterBorn(this, triggerWave);
                }
            }
        }
    }
}
