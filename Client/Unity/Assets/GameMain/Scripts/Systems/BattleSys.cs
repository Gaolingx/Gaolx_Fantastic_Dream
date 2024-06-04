//���ܣ�ս��ҵ��ϵͳ

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class BattleSys : SystemRoot
    {
        public static BattleSys Instance = null;
        public PlayerCtrlWnd playerCtrlWnd;
        public BattleEndWnd battleEndWnd;
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

            //��ΪGameRoot��������
            go.transform.SetParent(GameRoot.Instance.transform);
            battleMgr = go.AddComponent<BattleMgr>();

            playerInputObj.gameObject.SetActive(true);
            battleMgr.playerInputObj = playerInputObj;

            battleMgr.Init(mapid);
            SetPlayerCtrlWndState();

            GameRoot.Instance.SetGameState(GameState.FBFight);
        }

        public void EndBattle(bool isWin, int restHP)
        {
            playerCtrlWnd.SetWndState(false);
            GameRoot.Instance.dynamicWnd.RmvAllHpItemInfo();

            if (isWin)
            {
                //ս��ʤ�������ͽ���ս������
            }
            else
            {
                SetBattleEndWndState(FBEndType.Lose);
            }
        }

        public void EnterMainCity()
        {
            MainCitySys.Instance.EnterMainCity();
        }

        public void DestroyBattle()
        {
            SetPlayerCtrlWndState(false);
            SetBattleEndWndState(FBEndType.None, false);
            GameRoot.Instance.dynamicWnd.RmvAllHpItemInfo();
            Destroy(battleMgr.gameObject);
        }

        public void SetPlayerCtrlWndState(bool isActive = true)
        {
            playerCtrlWnd.SetWndState(isActive);
        }

        public void SetBattleEndWndState(FBEndType endType, bool isActive = true)
        {
            battleEndWnd.SetWndType(endType);
            battleEndWnd.SetWndState(isActive);
        }

        public void SetPlayerMoveDir(Vector2 dir)
        {
            if (battleMgr != null)
            {
                battleMgr.SetSelfPlayerMoveDir(dir);
            }
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
}
