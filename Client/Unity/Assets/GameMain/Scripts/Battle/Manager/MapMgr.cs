﻿//功能：地图管理器

using DarkGod.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class MapMgr : MonoBehaviour
    {
        private List<TriggerData> triggerLst;
        private int waveIndex = 1; //默认生成第一波怪物
        private BattleMgr battleMgr;

        private void InitTriggerData()
        {
            TriggerData[] triggerData = GameObject.FindObjectsOfType<TriggerData>();
            triggerLst = new List<TriggerData>(triggerData);

            for (int i = 0; i < triggerLst.Count; i++)
            {
                triggerLst[i].mapMgr = this;
                triggerLst[0].GetComponent<BoxCollider>().isTrigger = true;
            }
        }

        public void Init(BattleMgr battle)
        {
            battleMgr = battle;
            InitTriggerData();

            UIItemUtils.SetGameObjectTrans(this.gameObject, Vector3.zero, Quaternion.Euler(Vector3.zero), Vector3.one, false, false);

            //实例化第一批怪物
            battleMgr.LoadMonsterByWaveID(waveIndex);

            PECommon.Log("Init MapMgr Done.");
        }

        public void TriggerMonsterBorn(TriggerData trigger, int waveIndex)
        {
            if (battleMgr != null)
            {
                //修改碰撞环境(Trigger->Collider)
                BoxCollider boxCollider = trigger.GetComponent<BoxCollider>();
                boxCollider.isTrigger = false;

                //生成对应批次怪物
                battleMgr.LoadMonsterByWaveID(waveIndex);
                battleMgr.ActiveCurrentBatchMonsters();

                //激活触发器检测
                battleMgr.SetTriggerCheck(true);
            }
        }

        public bool SetNextTriggerOn()
        {
            waveIndex += 1;
            for (int i = 0; i < triggerLst.Count; i++)
            {
                //匹配对应TriggerData
                if (triggerLst[i].triggerWave == waveIndex)
                {
                    BoxCollider boxCollider = triggerLst[i].GetComponent<BoxCollider>();
                    boxCollider.isTrigger = true;
                    return true;
                }
            }

            return false;
        }
    }
}