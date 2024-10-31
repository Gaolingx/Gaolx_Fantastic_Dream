//功能：主城UI界面

using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class MainCityWnd : WindowRoot, IWindowRoot
    {
        #region UIDefine
        public Image imgTouch;
        public Image imgDirBg;
        public Image imgDirPoint;

        public Animation menuAni;
        public Button btnMenu;

        public TMP_Text txtFight;
        public TMP_Text txtPower;
        public Image imgPowerPrg;
        public TMP_Text txtLevel;
        public TMP_Text txtName;
        public TMP_Text txtExpPrg;

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

        public void OnEnable()
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
            SetText(txtPower, "体力:" + pd.power + "/" + PECommon.GetPowerLimit(pd.lv));
            imgPowerPrg.fillAmount = pd.power * 1.0f / PECommon.GetPowerLimit(pd.lv);
            SetText(txtLevel, pd.lv);
            SetText(txtName, pd.name);

            SetExpprg(pd, txtExpPrg, expPrgTrans);

            //设置自动任务图标
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
            //根据不同的npcID获取不同的图片(获取对应NPC的图片路径)
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

            //加载路径中的图片，并显示到Button内
            SetSprite(img, spPath);
        }
        #endregion

        #region ClickEvts
        public void ClickFubenBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIOpenPage);
            MainCitySys.MainInstance.EnterFuben();
        }
        public void ClickTaskBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIOpenPage);
            MainCitySys.MainInstance.OpenTaskRewardWnd();
        }
        public void ClickBuyPowerBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIOpenPage);
            MainCitySys.MainInstance.OpenBuyWnd(Constants.BuyTypePower);
        }
        public void ClickMKCoinBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIOpenPage);
            MainCitySys.MainInstance.OpenBuyWnd(Constants.MakeTypeCoin);
        }
        public void ClickStrongBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIOpenPage);
            MainCitySys.MainInstance.OpenStrongWnd();
        }
        public void ClickGuideBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            if (curtTaskData != null)
            {
                //任务存在，执行引导任务
                MainCitySys.MainInstance.RunTask(curtTaskData);
            }
            else
            {
                EventMgr.MainInstance.ShowMessageBox(this, new("更多引导任务，正在开发中..."));
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
            MainCitySys.MainInstance.OpenInfoWnd();
        }
        public void ClickChatBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIOpenPage);
            MainCitySys.MainInstance.OpenChatWnd();
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
                MainCitySys.MainInstance.SetMoveDir(Vector2.zero);
            });
            //摇杆拖动
            OnDrag(imgTouch.gameObject, (PointerEventData evt) =>
            {
                //计算拖拽的方向向量
                Vector2 dragDir = evt.position - startPos;
                float dragLen = dragDir.magnitude;

                //限制摇杆移动距离
                if (dragLen > pointDis)
                {
                    Vector2 clampDragDir = Vector2.ClampMagnitude(dragDir, pointDis);
                    imgDirPoint.transform.position = startPos + clampDragDir;
                }
                else
                {
                    imgDirPoint.transform.position = evt.position;
                }
                MainCitySys.MainInstance.SetMoveDir(dragDir.normalized);
            });
        }
        #endregion

        public void ClickSettingsBtn()
        {
            MainCitySys.MainInstance.OpenSettingsWnd();
        }

        public void OnDisable()
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

        public void ClickCloseBtn()
        {

        }
    }
}
