namespace DarkGod.Main
{
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

    public Text txtFight;
    public Text txtPower;
    public Image imgPowerPrg;
    public Text txtLevel;
    public Text txtName;
    public Text txtExpPrg;

    public Transform expPrgTrans;

    public Button btnGuide;
    #endregion

    private bool menuState = true;
    private float pointDis;
    private Vector2 startPos = Vector2.zero;
    private Vector2 defaultPos = Vector2.zero;
    private AutoGuideCfg curtTaskData;

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

    public void RefreshUI()
    {
        PlayerData pd = GameRoot.Instance.PlayerData;

        SetText(txtFight, PECommon.GetFightByProps(pd));
        SetText(txtPower, "����:" + pd.power + "/" + PECommon.GetPowerLimit(pd.lv));
        imgPowerPrg.fillAmount = pd.power * 1.0f / PECommon.GetPowerLimit(pd.lv);
        SetText(txtLevel, pd.lv);
        SetText(txtName, pd.name);


        #region Expprg
        int expPrgVal = (int)(pd.exp * 1.0f / PECommon.GetExpUpValByLv(pd.lv) * 100);
        //���������ȵ���ʾ
        SetText(txtExpPrg, expPrgVal + "%");

        int expPrgindex = expPrgVal / 10;

        GridLayoutGroup expGrid = expPrgTrans.GetComponent<GridLayoutGroup>();

        //ͨ�� ��׼��Ļ�߶�/ʵ���豸��Ļ�߶ȣ��������ǰUI����ڵ�ǰ��Ļ��Ҫ���ŵı�����ע��Canvas Scaler ҲҪ���ڸ߶���Ϊ���ű�׼��
        float globalRate = 1.0f * Constants.ScreenStandardWidth / Screen.width;
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
        #endregion

        //�����Զ�����ͼ��
        curtTaskData = resSvc.GetAutoGuideCfg(pd.guideid);
        if(curtTaskData != null)
        {
            SetGuideBtnIcon(curtTaskData.npcID);
        }
        else
        {
            SetGuideBtnIcon(Constants.DefaultGuideBtnIconID);
        }

    }

    private void SetGuideBtnIcon(int npcID)
    {
        string spPath = "";
        Image img = btnGuide.GetComponent<Image>();
        //���ݲ�ͬ��npcID��ȡ��ͬ��ͼƬ(��ȡ��ӦNPC��ͼƬ·��)
        switch(npcID)
        {
            case Constants.NPCWiseMan:
                spPath = PathDefine.WiseManHead;
                break;
            case Constants.NPCGeneral:
                spPath = PathDefine.GeneralHead;
                break;
            case Constants.NPCArtisan:
                spPath = PathDefine.ArtisanHead;
                break;
            case Constants.NPCTrader:
                spPath = PathDefine.TraderHead;
                break;
            default:
                spPath = PathDefine.TaskHead;
                break;
        }

        //����·���е�ͼƬ������ʾ��Button��
        SetSprite(img, spPath);
    }
    #endregion

    #region ClickEvts
    public void ClickFubenBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIOpenPage);
        MainCitySys.Instance.EnterFuben();
    }
    public void ClickTaskBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIOpenPage);
        MainCitySys.Instance.OpenTaskRewardWnd();
    }
    public void ClickBuyPowerBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIOpenPage);
        MainCitySys.Instance.OpenBuyWnd(Constants.BuyTypePower);
    }
    public void ClickMKCoinBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIOpenPage);
        MainCitySys.Instance.OpenBuyWnd(Constants.MakeTypeCoin);
    }
    public void ClickStrongBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIOpenPage);
        MainCitySys.Instance.OpenStrongWnd();
    }
    public void ClickGuideBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        if(curtTaskData != null)
        {
            //������ڣ�ִ����������
            MainCitySys.Instance.RunTask(curtTaskData);
        }
        else
        {
            GameRoot.AddTips("���������������ڿ�����...");
        }
    }
    public void ClickMenuBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIExtenBtn);

        menuState = !menuState;
        AnimationClip clip = null;
        if (menuState)
        {
            clip = menuAni.GetClip(Constants.openMCmenuAniClipName);
        }
        else
        {
            clip = menuAni.GetClip(Constants.closeMCmenuAniClipName);
        }
        menuAni.Play(clip.name);
    }
    public void ClickHeadBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIOpenPage);
        MainCitySys.Instance.OpenInfoWnd();
    }
    public void ClickChatBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIOpenPage);
        MainCitySys.Instance.OpenChatWnd();
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
            MainCitySys.Instance.SetMoveDir(Vector2.zero);
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
            MainCitySys.Instance.SetMoveDir(dragDir.normalized);
        });
    }
    #endregion

    public void ClickSettingsBtn()
    {
        MainCitySys.Instance.OpenSettingsWnd();
    }
}

}