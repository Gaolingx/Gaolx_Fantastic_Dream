//功能：UI界面基类

using System.Collections;
using System.Collections.Generic;
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

        protected void SetActive(GameObject go, bool isActive = true)
        {
            go.SetActive(isActive);
        }
        protected void SetActive(Transform trans, bool state = true)
        {
            trans.gameObject.SetActive(state);
        }
        protected void SetActive(RectTransform rectTrans, bool state = true)
        {
            rectTrans.gameObject.SetActive(state);
        }
        protected void SetActive(Image img, bool state = true)
        {
            img.transform.gameObject.SetActive(state);
        }
        protected void SetActive(Text txt, bool state = true)
        {
            txt.transform.gameObject.SetActive(state);
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
            T t = go.GetComponent<T>();
            if (t == null)
            {
                t = go.AddComponent<T>();
            }
            return t;
        }

        protected Transform GetTrans(Transform trans, string name)
        {
            if (trans != null)
            {
                return trans.Find(name);
            }
            else
            {
                return transform.Find(name);
            }
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

        #region Click Evts
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

        //Reload Cfg Data
        protected void ClickResetCfgsBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            configSvc.ResetSkillCfgs();
            MsgBox.MainInstance.ShowMessageBox("技能数据重置成功！");
        }

        protected void ExitCurrentBattle(bool enterFubenWndIfNeed = false)
        {
            if (GameRoot.MainInstance.GetGameState() == GameState.FBFight)
            {
                BattleSys.Instance.EnterMainCity();
                BattleSys.Instance.DestroyBattle();
            }
            else if (GameRoot.MainInstance.GetGameState() == GameState.MainCity)
            {
                MsgBox.MainInstance.ShowMessageBox("当前未处于副本战斗关卡");
            }
        }
    }
}
