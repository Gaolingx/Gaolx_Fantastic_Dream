//功能：玩家控制界面

using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerCtrlWnd : WindowRoot
{
    public Image imgTouch;
    public Image imgDirBg;
    public Image imgDirPoint;
    public Text txtLevel;
    public Text txtName;
    public Text txtExpPrg;
    public Transform expPrgTrans;

    private float pointDis;
    private Vector2 startPos = Vector2.zero;
    private Vector2 defaultPos = Vector2.zero;

    protected override void InitWnd()
    {
        base.InitWnd();

        pointDis = Screen.height * 1.0f / Constants.ScreenStandardHeight * Constants.ScreenOPDis;
        defaultPos = imgDirBg.transform.position;
        SetActive(imgDirPoint, false);

        RegisterTouchEvts();
        RefreshUI();
    }

    //注册触摸事件
    public void RegisterTouchEvts()
    {
        //摇杆按下
        OnClickDown(imgTouch.gameObject, (PointerEventData evt) =>
        {
            startPos = evt.position;
            SetActive(imgDirPoint);
            imgDirBg.transform.position = evt.position;
        });
        //摇杆抬起
        OnClickUp(imgTouch.gameObject, (PointerEventData evt) =>
        {
            imgDirBg.transform.position = defaultPos;
            SetActive(imgDirPoint, false);
            imgDirPoint.transform.localPosition = Vector2.zero;
            //MainCitySys.Instance.SetMoveDir(Vector2.zero);
        });
        //摇杆拖动
        OnDrag(imgTouch.gameObject, (PointerEventData evt) =>
        {
            Vector2 dragDir = evt.position - startPos;
            float dragLen = dragDir.magnitude;

            if (dragLen > pointDis)
            {
                Vector2 clampDragDir = Vector2.ClampMagnitude(dragDir, pointDis);
                imgDirPoint.transform.position = startPos + clampDragDir;
            }
            else
            {
                imgDirPoint.transform.position = evt.position;
            }
            //MainCitySys.Instance.SetMoveDir(dragDir.normalized);
        });
    }

    public void RefreshUI()
    {
        PlayerData pd = GameRoot.Instance.PlayerData;

        SetText(txtLevel, pd.lv);
        SetText(txtName, pd.name);

        #region Expprg
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
        #endregion
    }
}
