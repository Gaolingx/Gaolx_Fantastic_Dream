//功能：主城UI界面
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCityWnd : WindowRoot
{
    public Text txtFight;
    public Text txtPower;
    public Image imgPowerPrg;
    public Text txtLevel;
    public Text txtName;
    public Text txtExpPrg;

    public Transform expPrgTrans;

    protected override void InitWnd()
    {
        base.InitWnd();

        RefreshUI();
    }

    private void RefreshUI()
    {
        PlayerData pd = GameRoot.Instance.PlayerData;

        SetText(txtFight, PECommon.GetFightByProps(pd));
        SetText(txtPower, "体力:" + pd.power + "/" + PECommon.GetPowerLimit(pd.lv));
        imgPowerPrg.fillAmount = pd.power * 1.0f / PECommon.GetPowerLimit(pd.lv);
        SetText(txtLevel, pd.lv);
        SetText(txtName, pd.name);

        //expPrg

        GridLayoutGroup expGrid = expPrgTrans.GetComponent<GridLayoutGroup>();

        //通过 标准屏幕高度/实际设备屏幕高度，计算出当前UI相对于当前屏幕需要缩放的比例（注意Canvas Scaler 也要基于高度作为缩放标准）
        float globalRate = 1.0f * Constants.ScreenStandardHeight / Screen.height;
        //算出屏幕真实宽度
        float screenWidth = Screen.width * globalRate;
        //减去小的间隙
        float expCellWidth = (screenWidth - 180) / 10;

        expGrid.cellSize = new Vector2(expCellWidth, 7);

    }
}
