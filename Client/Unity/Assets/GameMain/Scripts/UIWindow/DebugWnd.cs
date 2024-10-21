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
        public Transform RuntimeHierarchy, RuntimeInspector;

        protected override void InitWnd()
        {
            base.InitWnd();

        }

        public void OnEnable()
        {
            btnCloseDebugItem.onClick.AddListener(delegate { ClickCloseBtn(); });
            FpsWndToggle.onValueChanged.AddListener(ClickFpsWndToggle);
            RuntimeHierarchyToggle.onValueChanged.AddListener(ClickRuntimeHierarchyToggle);
            RuntimeInspectorToggle.onValueChanged.AddListener(ClickRuntimeInspectorToggle);
        }

        private void ActiveDebugItemWnd(bool active = true)
        {
            SetWndState(active);
            btnCloseDebugItem.gameObject.SetActive(active);
        }

        public void ClickFpsWndToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            ActiveDebugItemWnd();
            fpsWnd.gameObject.SetActive(val);
        }

        public void ClickRuntimeHierarchyToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            ActiveDebugItemWnd();
            RuntimeHierarchy.gameObject.SetActive(val);
        }

        public void ClickRuntimeInspectorToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            ActiveDebugItemWnd();
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
            fpsWnd.gameObject.SetActive(false);
            RuntimeHierarchy.gameObject.SetActive(false);
            RuntimeInspector.gameObject.SetActive(false);

            FpsWndToggle.isOn = false;
            RuntimeHierarchyToggle.isOn = false;
            RuntimeInspectorToggle.isOn = false;

            ActiveDebugItemWnd(false);
        }
    }
}
