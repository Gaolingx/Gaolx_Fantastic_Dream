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
    public Image imgCurtPos; //��ǰѡ��λ�õ�ͼƬ
    public Text txtStartLv; //��ǰ�Ǽ�����
    public Transform starTransGrp; //���Ǹ������Transform
    public Text propHP1; //Ѫ��
    public Text propHurt1; //�˺�
    public Text propDef1; //����
    public Text propHP2;
    public Text propHurt2;
    public Text propDef2;
    public Image propArr1; //��ͷ
    public Image propArr2;
    public Image propArr3;

    public Text txtNeedLv; //�����������͵ȼ�
    public Text txtCostCoin; //���ĵĽ��
    public Text txtCostCrystal; //���ĵ�ˮ��

    public Transform costTransRoot;
    public Text txtCoin; //��ǰӵ�еĽ��
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

        RefreshItem();
    }

    private void RefreshItem()
    {
        //���
        SetText(txtCoin, pd.coin);
        switch(currentIndex)
        {
            case 0:
                SetSprite(imgCurtPos, PathDefine.ItemToukui);
                break;
            case 1:
                SetSprite(imgCurtPos, PathDefine.ItemBody);
                break;
            case 2:
                SetSprite(imgCurtPos, PathDefine.ItemYaobu);
                break;
            case 3:
                SetSprite(imgCurtPos, PathDefine.ItemHand);
                break;
            case 4:
                SetSprite(imgCurtPos, PathDefine.ItemLeg);
                break;
            case 5:
                SetSprite(imgCurtPos, PathDefine.ItemFoot);
                break;
            default:
                break;
        }
        //�Ǽ�
        SetText(txtStartLv, pd.strongArr[currentIndex] + "�Ǽ�");

        int curtStarLv = pd.strongArr[currentIndex];
        for (int i = 0; i < starTransGrp.childCount; i++)
        {
            //������ȡ�������Image���
            Image img = starTransGrp.GetChild(i).GetComponent<Image>();
            if (i < curtStarLv)
            {
                //�����ǰ�������Ǽ�ͼƬ<��ǰ�Ǽ���˵��û�ﵽ���¼�����ʾ�յ��Ǽ�ͼƬ
                SetSprite(img, PathDefine.SpStar2);
            }
            else
            {
                //������ʾʵ���Ǽ�ͼƬ
                SetSprite(img, PathDefine.SpStar1);
            }
        }

        //��ǰ�Ǽ��������Լӳ�
        //resSvc.GetPropAddValPreLv(��ǰ��λ, ��ǰ�Ǽ�, ����)
        int sumAddHp = resSvc.GetPropAddValPreLv(currentIndex, curtStarLv, 1);
        int sumAddHurt = resSvc.GetPropAddValPreLv(currentIndex, curtStarLv, 2);
        int sumAddDef = resSvc.GetPropAddValPreLv(currentIndex, curtStarLv, 3);
        SetText(propHP1, "����  +" + sumAddHp);
        SetText(propHurt1, "�˺�  +" + sumAddHurt);
        SetText(propDef1, "����  +" + sumAddDef);

        int nextStarLv = curtStarLv + 1;
        //��ȡ��һ�Ǽ���Ҫ��������ֵ
        StrongCfg nextSd = resSvc.GetStrongData(currentIndex, nextStarLv);
        if (nextSd != null)
        {
            SetActive(propHP2);
            SetActive(propHurt2);
            SetActive(propDef2);

            SetActive(costTransRoot);
            SetActive(propArr1);
            SetActive(propArr2);
            SetActive(propArr3);

            SetText(propHP2, "ǿ���� +" + nextSd.addhp);
            SetText(propHurt2, "+" + nextSd.addhurt);
            SetText(propDef2, "+" + nextSd.adddef);

            SetText(txtNeedLv, "��Ҫ�ȼ���" + nextSd.minlv);
            SetText(txtCostCoin, "��Ҫ���ģ�      " + nextSd.coin);

            SetText(txtCostCrystal, nextSd.crystal + "/" + pd.crystal);
        }
        else
        {
            //��ǰ�Ǽ������󣬲��ܽ���ǿ�����رջ������֡�����ǿ������ȡ�����Լӳɣ�
            SetActive(propHP2, false);
            SetActive(propHurt2, false);
            SetActive(propDef2, false);

            SetActive(costTransRoot, false);
            SetActive(propArr1, false);
            SetActive(propArr2, false);
            SetActive(propArr3, false);
        }
    }

    public void ClickCloseBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        SetWndState(false);
    }

}
