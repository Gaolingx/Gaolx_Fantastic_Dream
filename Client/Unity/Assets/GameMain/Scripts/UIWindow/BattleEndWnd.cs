//功能：战斗结算界面

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class BattleEndWnd : WindowRoot
    {
        #region UI Define
        public Transform rewardTrans;
        public Button btnClose;
        public Button btnExit;
        public Button btnSure;
        public Text txtTime;
        public Text txtRestHP;
        public Text txtReward;
        public Animation ani;
        #endregion


        protected override void InitWnd()
        {
            base.InitWnd();

            RefreshUI();
        }

        private void RefreshUI()
        {

        }

    }
}
