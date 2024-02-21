//���ܣ�ǿ����������
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

        //ע�����¼�
        RegClickEvts();

        ClickPosItem(0);
    }

    private void RegClickEvts()
    {
        //��ȡÿ���������е�image���
        for (int i = 0; i < posBtnTrans.childCount; i++)
        {
            Image img = posBtnTrans.GetChild(i).GetComponent<Image>();

            //��ӵ���¼�
            OnClick(img.gameObject, (object args) =>
            {
                ClickPosItem((int)args);
                audioSvc.PlayUIAudio(Constants.UIClickBtn);
            }, i);
            imgs[i] = img;
        }
    }

    //���ݵ��λ�ã�չʾ��ͬ��λ��Ϣ����Ҫ���ʱ����һ�����������ݲ�����ʾ��Ӧ������Ϣ
    private void ClickPosItem(int index)
    {
        PECommon.Log("Click Item:" + index);

        //�������飬���������һ����λ���䱳����Ϊ��ͷ������ı��ƽ��
        for (int i = 0;i < imgs.Length;i++)
        {
            Transform trans = imgs[i].transform;

            currentIndex = index;
            //�жϵ�ǰ�����ͼƬ�ͱ�����ͼƬ�Ƿ�һ��
            if(i == currentIndex)
            {
                //�����ȣ����ü�ͷ��ʾ
                SetSprite(imgs[i], PathDefine.ItemArrorBG);
                //����λ��
                trans.localPosition = new Vector3(10, trans.localPosition.y, 0);
                //���óߴ�
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
