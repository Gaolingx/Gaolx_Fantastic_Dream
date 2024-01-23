//功能：主城UI界面
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private float pointDis;
    private Vector2 startPos = Vector2.zero;
    private Vector2 defaultPos = Vector2.zero;

    #region MainFunctions
    protected override void InitWnd()
    {
        base.InitWnd();
        pointDis = Screen.height * 1.0f / Constants.ScreenStandardHeight * Constants.ScreenOPDis;
        defaultPos = imgDirBg.transform.position;
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
        //摇杆按下
        OnClickDown(imgTouch.gameObject, (PointerEventData evt) =>
        {
            //计算摇杆按下去后，拖拽的方向
            //方法是记录按下去时点击的位置(起始位置startPos)，拖拽时用拖拽后摇杆新的点减去当前按下去的点，算出方向向量
            startPos = evt.position;
            //按下去时激活被隐藏的摇杆点
            SetActive(imgDirPoint);
            //当摇杆按下时，其位置要更新到点击位置
            imgDirBg.transform.position = evt.position; //使用position的原因是点击事件传入的是全局坐标
        });
        //摇杆抬起
        OnClickUp(imgTouch.gameObject, (PointerEventData evt) =>
        {
            //当抬起时复原背景到初始位置（defaultPos）
            imgDirBg.transform.position = defaultPos;
            SetActive(imgDirPoint, false);
            //复原摇杆点的位置至正中央
            imgDirPoint.transform.localPosition = Vector2.zero; //使用localPosition的原因是imgDirPoint的坐标是相对于父物体坐标（本地坐标）

        });
        //摇杆拖动
        OnDrag(imgTouch.gameObject, (PointerEventData evt) =>
        {
            //计算拖拽的方向向量
            Vector2 dragDir = evt.position - startPos;
            float dragLen = dragDir.magnitude;

            //限制摇杆移动距离
            if(dragLen > pointDis)
            {
                Vector2 clampDragDir = Vector2.ClampMagnitude(dragDir, pointDis);
                imgDirPoint.transform.position = startPos + clampDragDir;
            }
            else
            {
                imgDirPoint.transform.position = evt.position; 
            }
        });
    }
    #endregion
}
