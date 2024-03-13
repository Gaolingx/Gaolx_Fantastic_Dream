//功能：计时服务

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

        //设置定时器日志输出
        pt.SetLog((string info) =>
        {
            PECommon.Log(info);
        });
        PECommon.Log("Init TimerSvc...");
    }

    //定时任务检测
    public void Update()
    {
        pt.Update();
    }

    //增加定时任务
    public int AddTimeTask(Action<int> callback, double delay, PETimeUnit timeUnit = PETimeUnit.Millisecond, int count = 1) //count为0将一直循环，直到手动取消
    {
        return pt.AddTimeTask(callback, delay, timeUnit, count); //返回任务id
    }

}
