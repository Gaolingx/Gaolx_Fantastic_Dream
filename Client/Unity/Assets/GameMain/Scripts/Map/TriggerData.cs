//���ܣ���ͼ����������

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
            //������ұ�ǩ
            if (other.gameObject.tag == Constants.CharPlayerWithTag)
            {
                //���õ�ͼ������������һ������
                if (mapMgr != null)
                {
                    mapMgr.TriggerMonsterBorn(this, triggerWave);
                }
            }
        }
    }
}
