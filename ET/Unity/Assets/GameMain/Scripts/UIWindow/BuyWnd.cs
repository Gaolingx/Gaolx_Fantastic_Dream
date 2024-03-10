//功能：购买交易窗口


using PEProtocol;
using UnityEngine;
using UnityEngine.UI;

public class BuyWnd : WindowRoot {
    public Text txtInfo;
    public Button btnSure;

    private int buyType;//0：体力 1：金币

    protected override void InitWnd()
    {
        base.InitWnd();
    }

    public void ClickCloseBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        SetWndState(false);
    }
}