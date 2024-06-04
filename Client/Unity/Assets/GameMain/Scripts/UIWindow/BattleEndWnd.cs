//功能：战斗结算界面

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public enum FBEndType
    {
        None,
        Pause, //副本暂停
        Win, //副本战斗胜利
        Lose
    }
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

        private FBEndType endType = FBEndType.None;

        protected override void InitWnd()
        {
            base.InitWnd();

            RefreshUI();
        }

        public void SetWndType(FBEndType endType)
        {
            this.endType = endType;
        }

        public void ClickCloseBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            BattleSys.Instance.battleMgr.SetPauseGame(false);
            SetWndState(false);
        }

        public void ClickExitBtn()
        {
            if (audioSvc != null)
            {
                audioSvc.PlayUIAudio(Constants.UIClickBtn);
            }
            else
            {
                AudioSvc.Instance.PlayUIAudio(Constants.UIClickBtn);
            }

            //进入主城，销毁当前战斗
            ExitCurrentBattle();
        }

        private void ExitCurrentBattle(bool enterFubenWndIfNeed = false)
        {
            if (GameRoot.Instance.GetGameState() == GameState.FBFight)
            {
                BattleSys.Instance.EnterMainCity();
                BattleSys.Instance.DestroyBattle();
            }
            else if (GameRoot.Instance.GetGameState() == GameState.MainCity)
            {
                GameRoot.AddTips("当前未处于副本战斗关卡");
            }
        }

        public void ClickSureBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            //进入主城，销毁当前战斗，打开副本界面
        }

        private bool FBEndTypePause()
        {
            SetActive(rewardTrans, false);
            SetActive(btnExit.gameObject);
            SetActive(btnClose.gameObject);

            return true;
        }
        private bool FBEndTypeWin()
        {
            return true;
        }
        private bool FBEndTypeLose()
        {
            return true;
        }

        private bool RefreshUI() => endType switch
        {
            FBEndType.Pause => FBEndTypePause(),
            FBEndType.Win => FBEndTypeWin(),
            FBEndType.Lose => FBEndTypeLose(),
            _ => throw new ArgumentOutOfRangeException(nameof(endType)),
        };

    }
}
