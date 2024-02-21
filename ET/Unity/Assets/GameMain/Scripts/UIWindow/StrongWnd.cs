//功能：强化升级界面
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PEProtocol;
using UnityEngine.EventSystems;

public class StrongWnd : WindowRoot
{
    #region UI Define
    public Image imgCurtPos;
    public Text txtStartLv;
    public Transform starTransGrp;
    public Text propHP1;
    public Text propHurt1;
    public Text propDef1;
    public Text propHP2;
    public Text propHurt2;
    public Text propDef2;
    public Image propArr1;
    public Image propArr2;
    public Image propArr3;

    public Text txtNeedLv;
    public Text txtCostCoin;
    public Text txtCostCrystal;

    public Transform costTransRoot;
    public Text txtCoin;
    #endregion

    #region Data Area
    public Transform posBtnTrans;
    private Image[] imgs = new Image[6];
    private int currentIndex;
    private PlayerData pd;
    //StrongCfg nextSd;

    #endregion

    protected override void InitWnd()
    {
        base.InitWnd();
        pd = GameRoot.Instance.PlayerData;

        //注册点击事件
        RegClickEvts();

        ClickPosItem(0);
    }

    private void RegClickEvts()
    {
        //获取每个子物体中的image组件
        for (int i = 0; i < posBtnTrans.childCount; i++)
        {
            Image img = posBtnTrans.GetChild(i).GetComponent<Image>();

            //添加点击事件
            OnClick(img.gameObject, (object args) =>
            {
                ClickPosItem((int)args);
                audioSvc.PlayUIAudio(Constants.UIClickBtn);
            }, i);
            imgs[i] = img;
        }
    }

    //根据点击位置，展示不同部位信息，需要点击时传入一个参数，根据参数显示相应数据信息
    private void ClickPosItem(int index)
    {
        PECommon.Log("Click Item:" + index);

        //遍历数组，当点击其中一个部位，其背景变为箭头，其余的变成平板
        for (int i = 0;i < imgs.Length;i++)
        {
            Transform trans = imgs[i].transform;

            currentIndex = index;
            //判断当前点击的图片和遍历的图片是否一致
            if(i == currentIndex)
            {
                //如果相等，则用箭头显示
                SetSprite(imgs[i], PathDefine.ItemArrorBG);
                //设置位置
                trans.localPosition = new Vector3(10, trans.localPosition.y, 0);
                //设置尺寸
                trans.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 95);
            }
            else
            {
                SetSprite(imgs[i], PathDefine.ItemPlatBG);
                trans.localPosition = new Vector3(0, trans.localPosition.y, 0);
                trans.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 85);
            }
        }
    }

    public void ClickCloseBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        SetWndState(false);
    }

}
