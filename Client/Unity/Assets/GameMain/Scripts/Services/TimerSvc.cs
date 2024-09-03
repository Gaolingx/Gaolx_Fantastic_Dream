//���ܣ���ʱ����

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;

namespace DarkGod.Main
{
    public class TimerSvc : Singleton<TimerSvc>
    {
        private PETimer pt;

        protected override void Awake()
        {
            base.Awake();
        }

        public void InitSvc()
        {
            pt = new PETimer();

            //���ö�ʱ����־���
            pt.SetLog((string info) =>
            {
                PECommon.Log(info);
            });
            PECommon.Log("Init TimerSvc...");
        }

        //��ʱ������
        public void Update()
        {
            pt.Update();
        }

        //���Ӷ�ʱ����
        public int AddTimeTask(System.Action<int> callback, double delay, PETimeUnit timeUnit = PETimeUnit.Millisecond, int count = 1)
        {
            return pt.AddTimeTask(callback, delay, timeUnit, count);
        }

        public double GetNowTime()
        {
            return pt.GetMillisecondsTime();
        }

        public void DelTask(int tid)
        {
            pt.DeleteTimeTask(tid);
        }

    }
}
