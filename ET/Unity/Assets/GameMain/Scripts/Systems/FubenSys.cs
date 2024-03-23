//功能：副本业务系统

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FubenSys : SystemRoot
{
    public static FubenSys Instance = null;

    public FubenWnd fubenWnd;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init FubenSys...");
    }

    public void EnterFuben()
    {
        OpenFubenWnd();
    }

    #region Fuben Wnd
    public void OpenFubenWnd()
    {
        fubenWnd.SetWndState();
    }
    #endregion

}
