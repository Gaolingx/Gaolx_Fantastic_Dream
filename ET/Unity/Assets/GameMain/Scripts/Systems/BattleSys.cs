//���ܣ�ս��ҵ��ϵͳ

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSys : SystemRoot
{
    public static BattleSys Instance = null;
    public PlayerCtrlWnd playerCtrlWnd;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init BattleSys...");
    }

    public void StartBattle(int mapid)
    {
        GameObject go = new GameObject
        {
            name = "BattleRoot"
        };

        //��ΪGameRoot��������
        go.transform.SetParent(GameRoot.Instance.transform);
        BattleMgr battleMgr = go.AddComponent<BattleMgr>();

        battleMgr.Init(mapid);
        SetPlayerCtrlWndState();
    }

    public void SetPlayerCtrlWndState(bool isActive = true)
    {
        playerCtrlWnd.SetWndState(isActive);
    }
}
