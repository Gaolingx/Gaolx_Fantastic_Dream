//功能：任务奖励界面


using PEProtocol;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TaskWnd : WindowRoot {
    public Transform scrollTrans;

    private PlayerData pd = null;

    protected override void InitWnd() {
        base.InitWnd();

        pd = GameRoot.Instance.PlayerData;
        RefreshUI();
    }

    public void RefreshUI() {
    }

    public void ClickCloseBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        SetWndState(false);
    }
}