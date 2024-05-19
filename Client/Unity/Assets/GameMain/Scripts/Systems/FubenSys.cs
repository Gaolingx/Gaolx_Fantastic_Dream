namespace DarkGod.Main
{
//���ܣ�����ҵ��ϵͳ

using PEProtocol;
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
        SetFubenWndState();
    }

    #region Fuben Wnd
    public void SetFubenWndState(bool isActive = true)
    {
        fubenWnd.SetWndState(isActive);
    }
    #endregion

    public void RspFBFight(GameMsg msg)
    {
        GameRoot.Instance.SetPlayerDataByFBStart(msg.rspFBFight);
        MainCitySys.Instance.maincityWnd.SetWndState(false);
        SetFubenWndState(false);
        //���ض�Ӧ��ս����������ʼ����ս������
        BattleSys.Instance.StartBattle(msg.rspFBFight.fbid);
    }

}

}