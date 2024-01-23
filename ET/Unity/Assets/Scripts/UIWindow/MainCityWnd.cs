//���ܣ�����UI����
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
        SetText(txtPower, "����:" + pd.power + "/" + PECommon.GetPowerLimit(pd.lv));
        imgPowerPrg.fillAmount = pd.power * 1.0f / PECommon.GetPowerLimit(pd.lv);
        SetText(txtLevel, pd.lv);
        SetText(txtName, pd.name);

        int expPrgVal = (int)(pd.exp * 1.0f / PECommon.GetExpUpValByLv(pd.lv) * 100);
        //���������ȵ���ʾ
        SetText(txtExpPrg, expPrgVal + "%");

        int expPrgindex = expPrgVal / 10;

        GridLayoutGroup expGrid = expPrgTrans.GetComponent<GridLayoutGroup>();

        //ͨ�� ��׼��Ļ�߶�/ʵ���豸��Ļ�߶ȣ��������ǰUI����ڵ�ǰ��Ļ��Ҫ���ŵı�����ע��Canvas Scaler ҲҪ���ڸ߶���Ϊ���ű�׼��
        float globalRate = 1.0f * Constants.ScreenStandardHeight / Screen.height;
        //�����Ļ��ʵ���
        float screenWidth = Screen.width * globalRate;
        //��ȥС�ļ�϶
        float expCellWidth = (screenWidth - 180) / 10;

        expGrid.cellSize = new Vector2(expCellWidth, 7);

        //��������expItem
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

    //ע�ᴥ���¼�
    public void RegisterTouchEvts()
    {
        //ҡ�˰���
        OnClickDown(imgTouch.gameObject, (PointerEventData evt) =>
        {
            //����ҡ�˰���ȥ����ק�ķ���
            //�����Ǽ�¼����ȥʱ�����λ��(��ʼλ��startPos)����קʱ����ק��ҡ���µĵ��ȥ��ǰ����ȥ�ĵ㣬�����������
            startPos = evt.position;
            //����ȥʱ������ص�ҡ�˵�
            SetActive(imgDirPoint);
            //��ҡ�˰���ʱ����λ��Ҫ���µ����λ��
            imgDirBg.transform.position = evt.position; //ʹ��position��ԭ���ǵ���¼��������ȫ������
        });
        //ҡ��̧��
        OnClickUp(imgTouch.gameObject, (PointerEventData evt) =>
        {
            //��̧��ʱ��ԭ��������ʼλ�ã�defaultPos��
            imgDirBg.transform.position = defaultPos;
            SetActive(imgDirPoint, false);
            //��ԭҡ�˵��λ����������
            imgDirPoint.transform.localPosition = Vector2.zero; //ʹ��localPosition��ԭ����imgDirPoint������������ڸ��������꣨�������꣩

        });
        //ҡ���϶�
        OnDrag(imgTouch.gameObject, (PointerEventData evt) =>
        {
            //������ק�ķ�������
            Vector2 dragDir = evt.position - startPos;
            float dragLen = dragDir.magnitude;

            //����ҡ���ƶ�����
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
