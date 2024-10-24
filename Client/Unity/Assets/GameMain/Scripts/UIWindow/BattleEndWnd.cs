//功能：战斗结算界面

using DG.Tweening;
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
    public class BattleEndWnd : WindowRoot, IWindowRoot
    {
        #region UI Define
        public Transform rewardTrans;
        public Button btnClose;
        public Button btnExit;
        public Button btnSure;
        public Text txtTime;
        public Text txtRestHP;
        public Text txtReward;
        public Image imgLogo;
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

        public void OnEnable()
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
            BattleSys.MainInstance.battleMgr.SetPauseGame(false, false);
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
            ExitCurrentBattle(delegate { FubenSys.MainInstance.EnterFuben(); });
        }

        private bool FBEndTypePause()
        {
            SetActive(rewardTrans, false);
            SetActive(btnExit);
            SetActive(btnClose);

            return true;
        }
        private bool FBEndTypeWin()
        {
            MapCfg cfg = configSvc.GetMapCfg(fbid);
            int min = costtime / 60;
            int sec = costtime % 60;
            int coin = cfg.coin;
            int exp = cfg.exp;
            int crystal = cfg.crystal;
            SetText(txtTime, "通关时间：" + min + ":" + sec);
            SetText(txtRestHP, "剩余血量：" + resthp);
            SetText(txtReward, "关卡奖励：" + GetTextWithHexColor(coin + "金币 ", TextColorCode.Green) + GetTextWithHexColor(exp + "经验 ", TextColorCode.Yellow) + GetTextWithHexColor(crystal + "水晶", TextColorCode.Blue));

            LogoShowAni();

            return true;
        }

        private void LogoShowAni()
        {
            Sequence se;
            se = DOTween.Sequence();
            se.SetAutoKill(false);

            SetActive(rewardTrans, true);
            SetActive(btnExit, false);
            SetActive(btnClose, false);
            SetActive(imgLogo, false);
            SetActive(btnSure, false);
            CanvasGroup[] canvasGroups = rewardTrans.GetComponentsInChildren<CanvasGroup>();

            for (int i = 0; i < canvasGroups.Length; i++)
            {
                SetActive(canvasGroups[i], true);
            }

            se.Append(canvasGroups[0].DOFade(0, 1f).From().SetDelay(1f));
            se.Insert(0.5f, canvasGroups[0].DOFade(0, 1f).From())
               .Join(canvasGroups[0].GetComponent<RectTransform>().DOAnchorPosX(850f, 1f).From(true).SetEase(Ease.OutExpo).SetDelay(0.5f))
               .AppendCallback(() => { audioSvc.PlayUIAudio(Constants.FBItemEnter); })
               .Join(canvasGroups[1].DOFade(0, 1f).From())
               .Join(canvasGroups[1].GetComponent<RectTransform>().DOAnchorPosX(850f, 1f).From(true).SetEase(Ease.OutExpo))
               .AppendCallback(() => { audioSvc.PlayUIAudio(Constants.FBItemEnter); })
               .Join(canvasGroups[2].DOFade(0, 1f).From())
               .Join(canvasGroups[2].GetComponent<RectTransform>().DOAnchorPosX(850f, 1f).From(true).SetEase(Ease.OutExpo))
               .AppendCallback(() => { audioSvc.PlayUIAudio(Constants.FBItemEnter); });

            se.AppendInterval(0.5f);
            se.AppendCallback(() => { SetActive(imgLogo, true); });

            se.AppendInterval(2f);
            se.AppendCallback(() => { SetActive(btnSure, true); });

        }

        private bool FBEndTypeLose()
        {
            SetActive(rewardTrans, false);
            SetActive(btnExit);
            SetActive(btnClose, false);
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

        public void OnDisable()
        {
            btnClose.onClick.RemoveAllListeners();
            btnExit.onClick.RemoveAllListeners();
            btnSure.onClick.RemoveAllListeners();
        }

    }
}
