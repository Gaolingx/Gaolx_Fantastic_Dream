using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class FpsWnd : WindowRoot
    {
        public Transform FpsWindowScript;

        private bool _isActive = false;
        protected override void InitWnd()
        {
            base.InitWnd();

        }
        public void ClickShowDebugInfoBtn(bool active = true)
        {
            _isActive = active;
            SetFpsWindowScriptActive();
        }

        private void SetFpsWindowScriptActive()
        {
            if (_isActive)
            {
                FpsWindowScript.gameObject.SetActive(true);
            }
            else
            {
                FpsWindowScript.gameObject.SetActive(false);
            }
        }
    }
}
