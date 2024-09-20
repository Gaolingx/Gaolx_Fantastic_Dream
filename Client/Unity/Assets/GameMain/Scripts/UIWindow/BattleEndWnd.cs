//功能：战斗结算界面

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

        #region BattleData
        private int fbid;
        private int costtime;
        private int resthp;
        public void SetBattleEndData(int fbid, int costtime, int resthp)
        {
            this.fbid = fbid;
            this.costtime = costtime;
            this.resthp = resthp;
        }
        #endregion

        protected override void InitWnd()
        {
            base.InitWnd();

            RefreshUI();
        }

        private void OnEnable()
        {
            btnClose.onClick.AddListener(delegate { ClickCloseBtn(); });
            btnExit.onClick.AddListener(delegate { ClickExitBtn(); });
            btnSure.onClick.AddListener(delegate { ClickSureBtn(); });
        }

        public void SetWndType(FBEndType endType)
        {
            this.endType = endType;
        }

        public void ClickCloseBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            BattleSys.Instance.battleMgr.SetPauseGame(false, false);
            SetWndState(false);
        }

        public void ClickExitBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            //进入主城，销毁当前战斗
            ExitCurrentBattle();
        }

        public void ClickSureBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            //进入主城，销毁当前战斗，打开副本界面
            ExitCurrentBattle();
            //打开副本界面
            FubenSys.Instance.EnterFuben();
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
            SetActive(rewardTrans, false);
            SetActive(btnExit.gameObject, false);
            SetActive(btnClose.gameObject, false);

            MapCfg cfg = configSvc.GetMapCfg(fbid);
            int min = costtime / 60;
            int sec = costtime % 60;
            int coin = cfg.coin;
            int exp = cfg.exp;
            int crystal = cfg.crystal;
            SetText(txtTime, "通关时间：" + min + ":" + sec);
            SetText(txtRestHP, "剩余血量：" + resthp);
            SetText(txtReward, "关卡奖励：" + GetTextWithHexColor(coin + "金币 ", TextColorCode.Green) + GetTextWithHexColor(exp + "经验 ", TextColorCode.Yellow) + GetTextWithHexColor(crystal + "水晶", TextColorCode.Blue));

            timerSvc.AddTimeTask((int tid) =>
            {
                SetActive(rewardTrans);
                ani.Play();
                timerSvc.AddTimeTask((int tid1) =>
                {
                    audioSvc.PlayUIAudio(Constants.FBItemEnter);
                    timerSvc.AddTimeTask((int tid2) =>
                    {
                        audioSvc.PlayUIAudio(Constants.FBItemEnter);
                        timerSvc.AddTimeTask((int tid3) =>
                        {
                            audioSvc.PlayUIAudio(Constants.FBItemEnter);
                            timerSvc.AddTimeTask((int tid5) =>
                            {
                                audioSvc.PlayUIAudio(Constants.FBLogoEnter);
                            }, 300);
                        }, 270);
                    }, 270);
                }, 325);
            }, 1000);
            return true;
        }
        private bool FBEndTypeLose()
        {
            SetActive(rewardTrans, false);
            SetActive(btnExit.gameObject);
            SetActive(btnClose.gameObject, false);
            audioSvc.PlayUIAudio(Constants.FBLose);

            return true;
        }

        private bool RefreshUI() => endType switch
        {
            FBEndType.Pause => FBEndTypePause(),
            FBEndType.Win => FBEndTypeWin(),
            FBEndType.Lose => FBEndTypeLose(),
            _ => throw new System.ArgumentOutOfRangeException(nameof(endType)),
        };

        private void OnDisable()
        {
            btnClose.onClick.RemoveAllListeners();
            btnExit.onClick.RemoveAllListeners();
            btnSure.onClick.RemoveAllListeners();
        }

    }
}
