//功能：副本业务系统

using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class FubenSys : SystemRoot<FubenSys>
    {
        public static FubenSys Instance = null;

        public FubenWnd fubenWnd;

        protected override void Awake()
        {
            base.Awake();

            GameRoot.MainInstance.OnGameEnter += InitSys;
        }

        public override void InitSys()
        {
            base.InitSys();

            Instance = MainInstance;
            PECommon.Log("Init FubenSys...");
        }

        public void EnterFuben()
        {
            SetFubenWndState();
        }

        #region Fuben Wnd
        public void SetFubenWndState(bool isActive = true)
        {
            fubenWnd.SetWndState(isActive);
        }
        #endregion

        public void RspFBFight(GameMsg msg)
        {
            GameRoot.MainInstance.SetPlayerDataByFBStart(msg.rspFBFight);
            MainCitySys.Instance.maincityWnd.SetWndState(false);
            SetFubenWndState(false);
            //加载对应的战斗场景，开始副本战斗任务
            BattleSys.Instance.StartBattle(msg.rspFBFight.fbid);
        }

        private void OnDestroy()
        {
            GameRoot.MainInstance.OnGameEnter -= InitSys;
        }
    }
}
