﻿//功能：业务系统基类

using HuHu;

namespace DarkGod.Main
{
    public class SystemRoot<T> : Singleton<T> where T : Singleton<T>
    {
        protected ResSvc resSvc;
        protected ConfigSvc configSvc;
        protected AudioSvc audioSvc;
        protected NetSvc netSvc;
        protected TimerSvc timerSvc;

        protected virtual void InitSys()
        {
            resSvc = ResSvc.MainInstance;
            configSvc = ConfigSvc.MainInstance;
            audioSvc = AudioSvc.MainInstance;
            netSvc = NetSvc.MainInstance;
            timerSvc = TimerSvc.MainInstance;
        }
    }
}
