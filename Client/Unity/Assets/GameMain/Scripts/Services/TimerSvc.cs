namespace DarkGod.Main
{
//���ܣ���ʱ����

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerSvc : SystemRoot
{
    public static TimerSvc Instance = null;

    private PETimer pt;

    public void InitSvc()
    {
        Instance = this;
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
    public int AddTimeTask(Action<int> callback, double delay, PETimeUnit timeUnit = PETimeUnit.Millisecond, int count = 1)
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