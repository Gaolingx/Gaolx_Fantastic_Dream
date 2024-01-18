//功能：主城UI界面
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCityWnd : WindowRoot
{
    #region UIDefine
    public Image imgTouch;
    public Image imgDirBg;
    public Image imgDirPoint;
    public Animation menuAni;
    public Button btnMenu;

    public string openMCmenuAniClipName = "OpenMCMenu";
    public string closeMCmenuAniClipName = "CloseMCMenu";
    public Text txtFight;
    public Text txtPower;
    public Image imgPowerPrg;
    public Text txtLevel;
    public Text txtName;
    public Text txtExpPrg;

    public Transform expPrgTrans;
    #endregion

    private bool menuState = true;

    #region MainFunctions
    protected override void InitWnd()
    {
        base.InitWnd();

        SetActive(imgDirPoint, false);

        RegisterTouchEvts();
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

        int expPrgVal = (int)(pd.exp * 1.0f / PECommon.GetExpUpValByLv(pd.lv) * 100);
        //经验条进度的显示
        SetText(txtExpPrg, expPrgVal + "%");

        int expPrgindex = expPrgVal / 10;

        GridLayoutGroup expGrid = expPrgTrans.GetComponent<GridLayoutGroup>();

        //通过 标准屏幕高度/实际设备屏幕高度，计算出当前UI相对于当前屏幕需要缩放的比例（注意Canvas Scaler 也要基于高度作为缩放标准）
        float globalRate = 1.0f * Constants.ScreenStandardHeight / Screen.height;
        //算出屏幕真实宽度
        float screenWidth = Screen.width * globalRate;
        //减去小的间隙
        float expCellWidth = (screenWidth - 180) / 10;

        expGrid.cellSize = new Vector2(expCellWidth, 7);

        //遍历所有expItem
        for (int i = 0; i < expPrgTrans.childCount; i++)
        {
            Image img = expPrgTrans.GetChild(i).GetComponent<Image>();
            if (i < expPrgindex)
            {
                img.fillAmount = 1;
            }
            else if (i == expPrgindex)
            {
                img.fillAmount = expPrgVal % 10 * 1.0f / 10;
            }
            else
            {
                img.fillAmount = 0;
            }
        }

    }
    #endregion

    #region ClickEvts
    public void ClickMenuBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIExtenBtn);

        menuState = !menuState;
        AnimationClip clip = null;
        if (menuState)
        {
            clip = menuAni.GetClip(openMCmenuAniClipName);
        }
        else
        {
            clip = menuAni.GetClip(closeMCmenuAniClipName);
        }
        menuAni.Play(clip.name);
    }

    //注册触摸事件
    public void RegisterTouchEvts()
    {

    }
    #endregion
}
