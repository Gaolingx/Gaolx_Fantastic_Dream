using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsWnd : WindowRoot
{
    public GameObject FpsWindow;
    public GameObject FpsWindowScript;
    public Button ShowDebugInfoBtn;

    protected override void InitWnd()
    {
        base.InitWnd();

    }
    public void ClickShowDebugInfoBtn()
    {

        if(FpsWindowScript.activeSelf == true)
        {
            SetFpsWindowScriptActive(false);
        }
        else
        {
            SetFpsWindowScriptActive();
        }
    }

    private void SetFpsWindowScriptActive(bool isFpsWndActive = true)
    {
        if (isFpsWndActive)
        {
            FpsWindowScript.SetActive(true);
        }
        else
        {
            FpsWindowScript.SetActive(false);
        }
    }
}
