//功能：UI界面基类

using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class WindowRoot : MonoBehaviour
    {
        protected ResSvc resSvc = null;
        protected ConfigSvc configSvc = null;
        protected AudioSvc audioSvc = null;
        protected NetSvc netSvc = null;
        protected TimerSvc timerSvc = null;
        protected PlayerPrefsSvc playerPrefsSvc = null;

        public void SetWndState(bool isActive = true)
        {
            if (gameObject.activeSelf != isActive)
            {
                //如果传入的参数与当前状态不相同则设置为目标状态
                SetActive(gameObject, isActive);
            }
            if (isActive)
            {
                InitWnd();
            }
            else
            {
                ClearWnd();
            }
        }

        public bool GetWndState()
        {
            return gameObject.activeSelf;
        }

        protected virtual void InitWnd()
        {
            //获取单例的引用
            resSvc = ResSvc.MainInstance;
            configSvc = ConfigSvc.MainInstance;
            audioSvc = AudioSvc.MainInstance;
            netSvc = NetSvc.MainInstance;
            timerSvc = TimerSvc.MainInstance;
            playerPrefsSvc = PlayerPrefsSvc.MainInstance;
        }

        protected virtual void ClearWnd()
        {
            //重置为空
            resSvc = null;
            configSvc = null;
            audioSvc = null;
            netSvc = null;
            timerSvc = null;
        }

        #region Tool Functions

        protected void SetActive(GameObject go, bool value = true)
        {
            bool tempState = go.activeSelf;
            UnityExtension.SetActive(go, value, ref tempState);
        }

        protected void SetActive(Component component, bool value = true)
        {
            GameObject go = component.gameObject;
            bool tempState = go.activeSelf;
            UnityExtension.SetActive(go, value, ref tempState);
        }

        protected void SetActive<T>(bool value = true) where T : UnityEngine.Component
        {
            T component = gameObject.GetComponent<T>();
            bool tempState = component.gameObject.activeSelf;
            UnityExtension.SetActive(component.gameObject, value, ref tempState);
        }

        protected void SetText(Text txt, string context = "NaN")
        {
            txt.text = context;
        }
        protected void SetText(Transform trans, int num = 0)
        {
            SetText(trans.GetComponent<Text>(), num);
        }
        protected void SetText(Transform trans, string context = "")
        {
            SetText(trans.GetComponent<Text>(), context);
        }
        protected void SetText(Text txt, int num = 0)
        {
            SetText(txt, num.ToString());
        }

        protected async void SetSprite(Image img, string path)
        {
            Sprite sp = await resSvc.LoadSpriteAsync(Constants.ResourcePackgeName, path, true);
            img.sprite = sp;
        }

        protected T GetOrAddComponect<T>(GameObject go) where T : Component
        {
            return UnityExtension.GetOrAddComponent<T>(go);
        }

        #endregion

        #region FindChildComponent

        public Transform FindChild(RectTransform rectTransform, string path)
        {
            return UnityExtension.FindChild(rectTransform, path);
        }

        public Transform FindChild(Transform trans, string path)
        {
            return UnityExtension.FindChild(trans, path);
        }

        public T FindChildComponent<T>(RectTransform rectTransform, string path) where T : Component
        {
            return UnityExtension.FindChildComponent<T>(rectTransform, path);
        }

        public T FindChildComponent<T>(Transform trans, string path) where T : Component
        {
            return UnityExtension.FindChildComponent<T>(trans, path);
        }

        #endregion

        #region RichText
        public static string GetTextWithHexColor(string text, TextColorCode textColor)
        {
            string result = "";
            string colorStart = "<color=";
            string colorEnd = ">";
            string textEnd = "</color>";
            switch (textColor)
            {
                case TextColorCode.White:
                    result = colorStart + Constants.ColorWhiteHex + colorEnd + text + textEnd;
                    break;
                case TextColorCode.Red:
                    result = colorStart + Constants.ColorRedHex + colorEnd + text + textEnd;
                    break;
                case TextColorCode.Green:
                    result = colorStart + Constants.ColorGreenHex + colorEnd + text + textEnd;
                    break;
                case TextColorCode.Blue:
                    result = colorStart + Constants.ColorBlueHex + colorEnd + text + textEnd;
                    break;
                case TextColorCode.Yellow:
                    result = colorStart + Constants.ColorYellowHex + colorEnd + text + textEnd;
                    break;
                default:
                    result = colorStart + Constants.ColorWhiteHex + colorEnd + text + textEnd;
                    break;
            }
            return result;
        }

        protected string GetTextWithBoldFont(string text)
        {
            return "<b>" + text + "</b>";
        }

        protected string GetTextWithItalicFont(string text)
        {
            return "<i>" + text + "</i>";
        }

        protected string GetTextWithSizeFont(string text, int sizeNum)
        {
            return "<size=" + sizeNum + ">" + text + "</size>";
        }
        #endregion

        #region UIEvent
        protected void OnClick(GameObject go, System.Action<object> cb, object args)
        {
            PEListener listener = GetOrAddComponect<PEListener>(go);
            listener.onClick = cb;
            listener.args = args;
        }

        protected void OnClickDown(GameObject go, System.Action<PointerEventData> cb)
        {
            PEListener listener = GetOrAddComponect<PEListener>(go);
            listener.onClickDown = cb;
        }

        protected void OnClickUp(GameObject go, System.Action<PointerEventData> cb)
        {
            PEListener listener = GetOrAddComponect<PEListener>(go);
            listener.onClickUp = cb;
        }

        protected void OnDrag(GameObject go, System.Action<PointerEventData> cb)
        {
            PEListener listener = GetOrAddComponect<PEListener>(go);
            listener.onDrag = cb;
        }
        #endregion

        protected float GetScreenWidth()
        {
            //通过 标准屏幕高度/实际设备屏幕高度，计算出当前UI相对于当前屏幕需要缩放的比例（注意Canvas Scaler 也要基于高度作为缩放标准）
            float globalRate = UIItemUtils.GetScreenScale().x;
            //算出屏幕真实宽度
            float screenWidth = Screen.width * globalRate;

            return screenWidth;
        }

        protected void InitDropdownOptionData(Dropdown dropdown, List<string> itemLst)
        {
            List<Dropdown.OptionData> qualitySelectDropdownOptionData = new List<Dropdown.OptionData>();

            foreach (var item in itemLst)
            {
                Dropdown.OptionData data = new Dropdown.OptionData();
                data.text = item;
                qualitySelectDropdownOptionData.Add(data);
            }

            dropdown.options = qualitySelectDropdownOptionData;
        }

        #region Common UI

        protected void PauseGameInWnd()
        {
            if (GameRoot.MainInstance.GetGameState() == GameState.FBFight)
            {
                BattleSys.MainInstance.battleMgr.SetPauseGame(false, false);
            }
            else if (GameRoot.MainInstance.GetGameState() == GameState.MainCity)
            {
                MainCitySys.MainInstance.PauseGameLogic(false);
            }
        }

        //Reload Cfg Data
        protected void ClickResetCfgsBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            configSvc.ResetSkillCfgs();
            EventMgr.MainInstance.ShowMessageBox(this, new("技能数据重置成功！"));
        }

        protected void ExitCurrentBattle(System.Action callback = null)
        {
            if (GameRoot.MainInstance.GetGameState() == GameState.FBFight)
            {
                BattleSys.MainInstance.EnterMainCity();
                BattleSys.MainInstance.DestroyBattle();
                callback?.Invoke();
            }
            else if (GameRoot.MainInstance.GetGameState() == GameState.MainCity)
            {
                EventMgr.MainInstance.ShowMessageBox(this, new("当前未处于副本战斗关卡"));
            }
        }

        protected virtual void ClickExitGame()
        {
            GameRoot.MainInstance.ExitGame();
        }

        #region Expprg
        protected void SetExpprg(PlayerData pd, Text txtExpPrg, Transform expPrgTrans)
        {
            int expPrgVal = (int)(pd.exp * 1.0f / PECommon.GetExpUpValByLv(pd.lv) * 100);
            //经验条进度的显示
            SetText(txtExpPrg, expPrgVal + "%");

            int expPrgindex = expPrgVal / 10;

            GridLayoutGroup expGrid = expPrgTrans.GetComponent<GridLayoutGroup>();

            //减去小的间隙
            float expCellWidth = (GetScreenWidth() - 180) / 10;

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

        #endregion
    }
}
