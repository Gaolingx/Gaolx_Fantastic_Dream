using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class DebugWnd : WindowRoot, IWindowRoot
    {
        public Toggle FpsWndToggle;
        public Button btnCloseDebugItem;
        public Transform fpsWnd;

        protected override void InitWnd()
        {
            base.InitWnd();

        }

        public void OnEnable()
        {
            btnCloseDebugItem.onClick.AddListener(delegate { ClickCloseBtn(); });
            FpsWndToggle.onValueChanged.AddListener(delegate (bool val) { ClickFpsWndToggle(val); });
        }

        public void ClickFpsWndToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            fpsWnd.gameObject.SetActive(val);
        }

        public void OnDisable()
        {
            btnCloseDebugItem.onClick.RemoveAllListeners();
            FpsWndToggle.onValueChanged.RemoveAllListeners();
        }

        public void ClickCloseBtn()
        {
            SetActive(fpsWnd, false);
            SetActive(btnCloseDebugItem, false);

            FpsWndToggle.isOn = false;

            SetWndState(false);
        }
    }
}
