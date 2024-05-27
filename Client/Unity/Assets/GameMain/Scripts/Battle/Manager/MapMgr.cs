//功能：地图管理器


using UnityEngine;

namespace DarkGod.Main
{
    public class MapMgr : MonoBehaviour
    {
        private int waveIndex = 1; //默认生成第一波怪物
        private BattleMgr battleMgr;

        public void Init(BattleMgr battle)
        {
            battleMgr = battle;

            //实例化第一批怪物
            battleMgr.LoadMonsterByWaveID(waveIndex);

            PECommon.Log("Init MapMgr Done.");
        }

        public void TriggerMonsterBorn(TriggerData trigger, int waveIndex)
        {
            if(battleMgr != null)
            {
                //修改碰撞环境(Trigger->Collider)
                BoxCollider boxCollider = trigger.gameObject.GetComponent<BoxCollider>();
                boxCollider.isTrigger = false;

                //生成对应批次怪物
                battleMgr.LoadMonsterByWaveID(waveIndex);
                battleMgr.ActiveCurrentBatchMonsters();
            }
        }
    }
}