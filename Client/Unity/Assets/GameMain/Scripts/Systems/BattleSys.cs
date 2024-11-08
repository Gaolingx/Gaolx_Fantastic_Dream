//功能：战斗业务系统

using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class BattleSys : SystemRoot<BattleSys>
    {
        public PlayerCtrlWnd playerCtrlWnd;
        public BattleMgr battleMgr;

        private int battleFbid;
        private double startTime;

        protected override void Awake()
        {
            base.Awake();

            EventMgr.MainInstance.OnGameEnter += delegate { InitSys(); };
        }

        public override void InitSys()
        {
            base.InitSys();

            PECommon.Log("Init BattleSys...");
        }

        public void StartBattle(int mapid)
        {
            battleFbid = mapid;
            GameObject go = new GameObject
            {
                name = "BattleRoot"
            };

            //成为GameRoot的子物体
            go.transform.SetParent(EventMgr.MainInstance.transform);
            battleMgr = go.AddComponent<BattleMgr>();

            battleMgr.Init(mapid, () =>
            {
                startTime = timerSvc.GetNowTime();
            });
            SetPlayerCtrlWndState();

            GameRoot.MainInstance.GameRootGameState = GameState.FBFight;
        }

        public void EndBattle(bool isWin, int restHP)
        {
            SetPlayerCtrlWndState(false);
            GameRoot.MainInstance.dynamicWnd.RmvAllHpItemInfo();

            if (isWin)
            {
                double endTime = timerSvc.GetNowTime();
                //战斗胜利，发送结算战斗请求
                GameMsg msg = new GameMsg
                {
                    cmd = (int)CMD.ReqFBFightEnd,
                    reqFBFightEnd = new ReqFBFightEnd
                    {
                        win = isWin,
                        fbid = battleFbid,
                        resthp = restHP,
                        costtime = (int)((endTime - startTime) / 1000)
                    }
                };

                netSvc.SendMsg(msg);
            }
            else
            {
                SetBattleEndWndState(FBEndType.Lose);
            }
        }

        public void EnterMainCity()
        {
            MainCitySys.MainInstance.EnterMainCity();
        }

        public void DestroyBattle()
        {
            SetPlayerCtrlWndState(false);
            SetBattleEndWndState(FBEndType.None, false);
            GameRoot.MainInstance.dynamicWnd.RmvAllHpItemInfo();
            Destroy(battleMgr.gameObject);
        }

        public void SetPlayerCtrlWndState(bool isActive = true)
        {
            playerCtrlWnd.SetWndState(isActive);
        }

        public void SetBattleEndWndState(FBEndType endType, bool isActive = true)
        {
            GameRoot.MainInstance.BattleEndWndAction?.Invoke(isActive, endType);
        }

        public void RspFightEnd(GameMsg msg)
        {
            RspFBFightEnd data = msg.rspFBFightEnd;
            GameRoot.MainInstance.SetPlayerDataByFBEnd(data);

            GameRoot.MainInstance.battleEndWnd.SetBattleEndData(data.fbid, data.costtime, data.resthp);
            SetBattleEndWndState(FBEndType.Win);
        }

        public void SetPlayerMoveDir(Vector2 dir)
        {
            if (EventMgr.MainInstance.CurrentEPlayer.Value != null)
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
            return GameRoot.MainInstance.starterAssetsInputs.move;
        }

        public bool CanRlsSkill()
        {
            return battleMgr.CanRlsSkill();
        }

        private void OnDisable()
        {
            EventMgr.MainInstance.OnGameEnter -= delegate { InitSys(); };
        }
    }
}
