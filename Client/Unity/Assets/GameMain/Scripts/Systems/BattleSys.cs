//功能：战斗业务系统

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSys : SystemRoot
{
    public static BattleSys Instance = null;
    public PlayerCtrlWnd playerCtrlWnd;
    public BattleMgr battleMgr;
    public Transform playerInputObj;

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

        //成为GameRoot的子物体
        go.transform.SetParent(GameRoot.Instance.transform);
        battleMgr = go.AddComponent<BattleMgr>();

        playerInputObj.gameObject.SetActive(true);
        battleMgr.playerInputObj = playerInputObj;

        battleMgr.Init(mapid);
        SetPlayerCtrlWndState();
    }

    public void SetPlayerCtrlWndState(bool isActive = true)
    {
        playerCtrlWnd.SetWndState(isActive);
    }

    public void SetPlayerMoveDir(Vector2 dir)
    {
        battleMgr.SetSelfPlayerMoveDir(dir);
    }

    public void ReqPlayerReleaseSkill(int skillIndex)
    {
        battleMgr.ReqPlayerReleaseSkill(skillIndex);
    }

    public Vector2 GetDirInput()
    {
        return playerCtrlWnd.GetCurrentDir();
    }

    public bool CanRlsSkill()
    {
        return battleMgr.CanRlsSkill();
    }
}
