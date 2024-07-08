//功能：业务系统基类
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class SystemRoot : MonoBehaviour
    {
        protected ResSvc resSvc;
        protected AudioSvc audioSvc;
        protected NetSvc netSvc;
        protected TimerSvc timerSvc;

        public virtual void InitSys()
        {
            resSvc = ResSvc.MainInstance;
            audioSvc = AudioSvc.MainInstance;
            netSvc = NetSvc.MainInstance;
            timerSvc = TimerSvc.MainInstance;
        }
    }
}
