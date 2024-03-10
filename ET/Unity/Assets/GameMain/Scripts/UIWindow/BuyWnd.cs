//功能：购买交易窗口


using PEProtocol;
using UnityEngine;
using UnityEngine.UI;

public class BuyWnd : WindowRoot {
    public Text txtInfo;
    public Button btnSure;

    private int buyType;//0：体力 1：金币

    //设置购买类型
    public void SetBuyType(int type)
    {
        this.buyType = type;
    }
    protected override void InitWnd()
    {
        base.InitWnd();

        RefreshUI();
    }

    public void RefreshUI()
    {
        //通过打开窗口时设置buyType，控制窗口的显示，实现不同业务模块共用ui窗口的目的
        switch(buyType)
        {
            case Constants.BuyTypePower:
                //体力
                txtInfo.text = "是否花费" + Constants.txtColor(Constants.BuyCostCrystalOnce + "钻石", TxtColor.Red) + "购买" + Constants.txtColor("100体力", TxtColor.Green) + "?";
                break;
            case Constants.MakeTypeCoin:
                txtInfo.text = "是否花费" + Constants.txtColor(Constants.BuyCostCrystalOnce + "钻石", TxtColor.Red) + "购买" + Constants.txtColor("1000金币", TxtColor.Green) + "?";
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
    }

    public void ClickCloseBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        SetWndState(false);
    }
}