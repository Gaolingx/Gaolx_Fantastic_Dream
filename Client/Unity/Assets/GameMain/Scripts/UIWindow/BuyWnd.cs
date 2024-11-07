//功能：购买交易窗口

using PEProtocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class BuyWnd : WindowRoot, IWindowRoot
    {
        public TMP_Text txtInfo;
        public Button btnSure;
        public Button btnClose;

        private int buyType;//0：体力 1：金币，与buyCfg的ID对应
        private BuyCfg buyCfg;

        //设置购买类型
        public void SetBuyType(int type)
        {
            this.buyType = type;
            GetBuyCfg(type);
        }
        protected override void InitWnd()
        {
            base.InitWnd();

            GameRoot.MainInstance.PauseGameUIAction?.Invoke(true);

            btnSure.interactable = true;

            RefreshUI();
        }

        public void OnEnable()
        {
            btnSure.onClick.AddListener(delegate { ClickSureBtn(); });
            btnClose.onClick.AddListener(delegate { ClickCloseBtn(); });
        }

        public void GetBuyCfg(int buyType)
        {
            buyCfg = ConfigSvc.MainInstance.GetBuyCfg(buyType);
        }

        public void RefreshUI()
        {

            //通过打开窗口时设置buyType，控制窗口的显示，实现不同业务模块共用ui窗口的目的
            switch (buyType)
            {
                case Constants.BuyTypePower:
                    //体力
                    txtInfo.text = "是否花费" + GetTextWithHexColor(buyCfg.buyCostDiamondOnce + "钻石", TextColorCode.Red) + "购买" + GetTextWithHexColor(buyCfg.amountEachPurchase + "体力", TextColorCode.Green) + "?";
                    break;
                case Constants.MakeTypeCoin:
                    txtInfo.text = "是否花费" + GetTextWithHexColor(buyCfg.buyCostDiamondOnce + "钻石", TextColorCode.Red) + "购买" + GetTextWithHexColor(buyCfg.amountEachPurchase + "金币", TextColorCode.Green) + "?";
                    //金币
                    break;
                default:
                    break;
            }
        }


        public void ClickSureBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            //发送网络购买消息
            GameMsg msg = new GameMsg
            {
                cmd = (int)CMD.ReqBuy,
                reqBuy = new ReqBuy
                {
                    type = buyType,
                    cost = buyCfg.buyCostDiamondOnce
                }
            };

            netSvc.SendMsg(msg);
            //关闭确定按钮交互，直到服务器回应消息，防止网络不稳定客户端响应不及时造成重复点击购买
            btnSure.interactable = false;
        }

        public void ClickCloseBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            GameRoot.MainInstance.PauseGameUIAction?.Invoke(false);
            SetWndState(false);
        }

        public void OnDisable()
        {
            btnSure.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
        }
    }
}