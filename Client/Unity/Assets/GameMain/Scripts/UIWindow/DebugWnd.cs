using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class DebugWnd : WindowRoot, IWindowRoot
    {
        public Toggle FpsWndToggle;
        public Toggle RuntimeInspectorToggle;
        public Toggle RuntimeHierarchyToggle;
        public Button btnCloseDebugItem;
        public Transform fpsWnd;
        public Transform RuntimeHierarchy;
        public Transform RuntimeInspector;

        protected override void InitWnd()
        {
            base.InitWnd();

        }

        public void OnEnable()
        {
            btnCloseDebugItem.onClick.AddListener(delegate { ClickCloseBtn(); });
            FpsWndToggle.onValueChanged.AddListener(delegate (bool val) { ClickFpsWndToggle(val); });
            RuntimeHierarchyToggle.onValueChanged.AddListener(delegate (bool val) { ClickRuntimeHierarchyToggle(val); });
            RuntimeInspectorToggle.onValueChanged.AddListener(delegate (bool val) { ClickRuntimeInspectorToggle(val); });
        }

        public void ClickFpsWndToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            fpsWnd.gameObject.SetActive(val);
        }

        public void ClickRuntimeHierarchyToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            RuntimeHierarchy.gameObject.SetActive(val);
        }

        public void ClickRuntimeInspectorToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            RuntimeInspector.gameObject.SetActive(val);
        }

        public void OnDisable()
        {
            btnCloseDebugItem.onClick.RemoveAllListeners();
            FpsWndToggle.onValueChanged.RemoveAllListeners();
            RuntimeHierarchyToggle.onValueChanged.RemoveAllListeners();
            RuntimeInspectorToggle.onValueChanged.RemoveAllListeners();
        }

        public void ClickCloseBtn()
        {
            SetActive(RuntimeHierarchy, false);
            SetActive(RuntimeInspector, false);
            SetActive(fpsWnd, false);
            SetActive(btnCloseDebugItem, false);

            FpsWndToggle.isOn = false;
            RuntimeHierarchyToggle.isOn = false;
            RuntimeInspectorToggle.isOn = false;

            SetWndState(false);
        }
    }
}
