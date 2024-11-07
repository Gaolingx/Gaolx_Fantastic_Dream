//功能：角色信息展示界面

using PEProtocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class InfoWnd : WindowRoot, IWindowRoot
    {
        #region UI Define
        public RawImage imgChar;

        public TMP_Text txtInfo;
        public TMP_Text txtExp;
        public Image imgExpPrg;
        public TMP_Text txtPower;
        public Image imgPowerPrg;

        public TMP_Text txtJob;
        public TMP_Text txtFight;
        public TMP_Text txtHP;
        public TMP_Text txtHurt;
        public TMP_Text txtDef;

        public Button btnClose;

        public Button btnDetail;
        public Button btnCloseDetail;
        public Transform transDetail;

        public TMP_Text dtxhp;
        public TMP_Text dtxad;
        public TMP_Text dtxap;
        public TMP_Text dtxaddef;
        public TMP_Text dtxapdef;
        public TMP_Text dtxdodge;
        public TMP_Text dtxpierce;
        public TMP_Text dtxcritical;
        #endregion

        private Vector2 startPos;

        protected override void InitWnd()
        {
            base.InitWnd();

            GameRoot.MainInstance.PauseGameUIAction?.Invoke(true);

            RegTouchEvts();

            SetActive(transDetail, false);
            RefreshUI();
        }

        public void OnEnable()
        {
            btnDetail.onClick.AddListener(delegate { ClickDetailBtn(); });
            btnClose.onClick.AddListener(delegate { ClickCloseBtn(); });
            btnCloseDetail.onClick.AddListener(delegate { ClickCloseDetailBtn(); });
        }

        //InfoWnd角色左右拖拽旋转效果：当按下鼠标拖动或者触摸滑动屏幕时，获取滑动的水平距离，然后将该距离映射到人物旋转属性上
        //监听触摸事件
        private void RegTouchEvts()
        {
            OnClickDown(imgChar.gameObject, (PointerEventData evt) =>
            {
                startPos = evt.position;
                MainCitySys.MainInstance.SetStartRoate();
            });
            OnDrag(imgChar.gameObject, (PointerEventData evt) =>
            {
                float roate = -(evt.position.x - startPos.x) * Constants.OnDragCharRoateSpeed;
                MainCitySys.MainInstance.SetPlayerRoate(roate);
            });
        }

        private void RefreshUI()
        {
            //获取玩家数据
            PlayerData pd = GameRoot.MainInstance.PlayerData;

            SetText(txtInfo, pd.name + " LV." + pd.lv);
            SetText(txtExp, pd.exp + "/" + PECommon.GetExpUpValByLv(pd.lv));
            imgExpPrg.fillAmount = pd.exp * 1.0F / PECommon.GetExpUpValByLv(pd.lv);
            SetText(txtPower, pd.power + "/" + PECommon.GetPowerLimit(pd.lv));
            imgPowerPrg.fillAmount = pd.power * 1.0F / PECommon.GetPowerLimit(pd.lv);

            SetText(txtJob, " 职业   暗夜刺客");
            SetText(txtFight, " 战力   " + PECommon.GetFightByProps(pd));
            SetText(txtHP, " 血量   " + pd.hp);
            SetText(txtHurt, " 伤害   " + (pd.ad + pd.ap));
            SetText(txtDef, " 防御   " + (pd.addef + pd.apdef));

            //detail TODO
            SetText(dtxhp, pd.hp);
            SetText(dtxad, pd.ad);
            SetText(dtxap, pd.ap);
            SetText(dtxaddef, pd.addef);
            SetText(dtxapdef, pd.apdef);
            SetText(dtxdodge, pd.dodge + "%");
            SetText(dtxpierce, pd.pierce + "%");
            SetText(dtxcritical, pd.critical + "%");

        }

        public void ClickCloseBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            GameRoot.MainInstance.PauseGameUIAction?.Invoke(false);
            MainCitySys.MainInstance.CloseInfoWnd();
        }

        public void ClickDetailBtn()
        {
            SetActive(transDetail);
        }
        public void ClickCloseDetailBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            SetActive(transDetail, false);
        }

        public void OnDisable()
        {
            btnDetail.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
            btnCloseDetail.onClick.RemoveAllListeners();
        }
    }
}