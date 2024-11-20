using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;

namespace DarkGod.Main
{
    public class MessageBox : Singleton<MessageBox>
    {
        public DynamicWnd dynamicWnd { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            EventMgr.MainInstance.OnGameEnter += delegate { InitSvc(); };
        }

        private void InitSvc()
        {
            dynamicWnd = transform.Find(Constants.Path_DynamicWnd_Obj).GetComponent<DynamicWnd>();

        }

        public void ActiveDynamicWnd(bool isActive = true)
        {
            if (dynamicWnd != null)
                dynamicWnd.SetWndState(isActive);
        }

        public void ShowMessage(string msg)
        {
            if (dynamicWnd != null)
                dynamicWnd.AddTips(msg);
        }

        public void SetHPVal(string key, int oldVal, int newVal)
        {
            if (dynamicWnd != null)
                dynamicWnd.SetHPVal(key, oldVal, newVal);
        }

        public void SetSelfDodge()
        {
            if (dynamicWnd != null)
                dynamicWnd.SetSelfDodge();
        }

        public void SetDodge(string key)
        {
            if (dynamicWnd != null)
                dynamicWnd.SetDodge(key);
        }

        public void SetCritical(string key, int critical)
        {
            if (dynamicWnd != null)
                dynamicWnd.SetCritical(key, critical);
        }

        public void SetHurt(string key, int hurt)
        {
            if (dynamicWnd != null)
                dynamicWnd.SetHurt(key, hurt);
        }

        public void AddHpItemInfo(string mName, Transform trans, int hp)
        {
            if (dynamicWnd != null)
                dynamicWnd.AddHpItemInfo(mName, trans, hp);
        }

        public void RmvHpItemInfo(string mName)
        {
            if (dynamicWnd != null)
                dynamicWnd.RmvHpItemInfo(mName);
        }

        public void RmvAllHpItemInfo()
        {
            if (dynamicWnd != null)
                dynamicWnd.RmvAllHpItemInfo();
        }

        private void OnDisable()
        {
            EventMgr.MainInstance.OnGameEnter -= delegate { InitSvc(); };
        }
    }
}
