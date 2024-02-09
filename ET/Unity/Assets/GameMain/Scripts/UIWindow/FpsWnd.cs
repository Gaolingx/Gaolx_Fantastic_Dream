using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsWnd : MonoBehaviour
{
    public GameObject FpsWindow;
    public GameObject FpsWindowScript;
    public Button ShowDebugInfoBtn;
    private bool fpsWndState;

    public void InitWnd()
    {
        FpsWindow.SetActive(true);
        SetFpsWindowScriptActive();
    }
    public void ClickShowDebugInfoBtn()
    {

        if(FpsWindowScript.activeSelf == true)
        {
            fpsWndState = false;
        }
        else
        {
            fpsWndState = true;
        }
        SetFpsWindowScriptActive();

    }

    private void SetFpsWindowScriptActive()
    {
        if (fpsWndState)
        {
            FpsWindowScript.SetActive(true);
        }
        else
        {
            FpsWindowScript.SetActive(false);
        }
    }
}
