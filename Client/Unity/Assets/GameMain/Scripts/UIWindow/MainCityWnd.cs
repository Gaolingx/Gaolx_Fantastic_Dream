//���ܣ�����UI����
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DarkGod.Main
{
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

        public Button btnBuyPower;
        public Button btnGuide;
        public Button btnHead;
        public Button btnTask;
        public Button btnArena;
        public Button btnMKCoin;
        public Button btnStrong;
        public Button btnSettings;
        public Button btnChat;
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

            pointDis = 1 / UIItemUtils.GetScreenScale().y * Constants.ScreenOPDis;
            defaultPos = imgDirBg.transform.position;
            SetActive(imgDirPoint, false);

            RegisterTouchEvts();
            RefreshUI();
        }

        private void OnEnable()
        {
            btnMenu.onClick.AddListener(delegate { ClickMenuBtn(); });
            btnBuyPower.onClick.AddListener(delegate { ClickBuyPowerBtn(); });
            btnGuide.onClick.AddListener(delegate { ClickGuideBtn(); });
            btnHead.onClick.AddListener(delegate { ClickHeadBtn(); });
            btnTask.onClick.AddListener(delegate { ClickTaskBtn(); });
            btnArena.onClick.AddListener(delegate { ClickFubenBtn(); });
            btnMKCoin.onClick.AddListener(delegate { ClickMKCoinBtn(); });
            btnStrong.onClick.AddListener(delegate { ClickStrongBtn(); });
            btnSettings.onClick.AddListener(delegate { ClickSettingsBtn(); });
            btnChat.onClick.AddListener(delegate { ClickChatBtn(); });
        }

        public void RefreshUI()
        {
            PlayerData pd = GameRoot.MainInstance.PlayerData;

            SetText(txtFight, PECommon.GetFightByProps(pd));
            SetText(txtPower, "����:" + pd.power + "/" + PECommon.GetPowerLimit(pd.lv));
            imgPowerPrg.fillAmount = pd.power * 1.0f / PECommon.GetPowerLimit(pd.lv);
            SetText(txtLevel, pd.lv);
            SetText(txtName, pd.name);

            SetExpprg(pd, txtExpPrg, expPrgTrans);

            //�����Զ�����ͼ��
            curtTaskData = configSvc.GetAutoGuideCfg(pd.guideid);
            if (curtTaskData != null)
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
            switch (npcID)
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

            if (curtTaskData != null)
            {
                //������ڣ�ִ����������
                MainCitySys.Instance.RunTask(curtTaskData);
            }
            else
            {
                MsgBox.MainInstance.ShowMessageBox("���������������ڿ�����...");
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
                if (dragLen > pointDis)
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

        private void OnDisable()
        {
            btnMenu.onClick.RemoveAllListeners();
            btnBuyPower.onClick.RemoveAllListeners();
            btnGuide.onClick.RemoveAllListeners();
            btnHead.onClick.RemoveAllListeners();
            btnTask.onClick.RemoveAllListeners();
            btnArena.onClick.RemoveAllListeners();
            btnMKCoin.onClick.RemoveAllListeners();
            btnStrong.onClick.RemoveAllListeners();
            btnSettings.onClick.RemoveAllListeners();
            btnChat.onClick.RemoveAllListeners();
        }
    }
}
