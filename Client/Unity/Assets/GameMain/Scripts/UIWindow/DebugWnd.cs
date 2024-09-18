using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class DebugWnd : WindowRoot
    {
        public Toggle FpsWndToggle, RuntimeInspectorToggle, RuntimeHierarchyToggle;
        public Button btnCloseDebugItem;
        public Transform DebugItem;
        public Transform fpsWnd;
        public Transform RuntimeHierarchy, RuntimeInspector;

        protected override void InitWnd()
        {
            base.InitWnd();

            btnCloseDebugItem.onClick.AddListener(delegate { ClickCloseDebugItemBtn(); });
            FpsWndToggle.onValueChanged.AddListener(ClickFpsWndToggle);
            RuntimeHierarchyToggle.onValueChanged.AddListener(ClickRuntimeHierarchyToggle);
            RuntimeInspectorToggle.onValueChanged.AddListener(ClickRuntimeInspectorToggle);
        }

        private void ActiveDebugItemWnd(bool active = true)
        {
            DebugItem.gameObject.SetActive(active);
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

        public void ClickCloseDebugItemBtn()
        {
            fpsWnd.gameObject.SetActive(false);
            RuntimeHierarchy.gameObject.SetActive(false);
            RuntimeInspector.gameObject.SetActive(false);
            ActiveDebugItemWnd(false);
        }

        private void OnDisable()
        {
            btnCloseDebugItem.onClick.RemoveAllListeners();
            FpsWndToggle.onValueChanged.RemoveAllListeners();
            RuntimeHierarchyToggle.onValueChanged.RemoveAllListeners();
            RuntimeInspectorToggle.onValueChanged.RemoveAllListeners();
        }
    }
}
