//功能：玩家控制界面

using PEProtocol;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerCtrlWnd : WindowRoot
{
    public Text txtLevel;
    public Text txtName;
    public Text txtExpPrg;
    public Transform expPrgTrans;

    private BattleMgr battleMgr;
    StarterAssetsInputs playerInput;

    private EntityPlayer wndEntitySelfPlayer;

    protected override void InitWnd()
    {
        base.InitWnd();

        battleMgr = BattleMgr.Instance;
        RefreshUI();
    }

    private void Update()
    {
        if (battleMgr.entitySelfPlayer != null)
        {
            wndEntitySelfPlayer = battleMgr.entitySelfPlayer;
            playerInput = wndEntitySelfPlayer.playerInput;

            ListeningTouchEvts();
            ListeningClickPlayerNormalAtk();
            ListeningClickPlayerSkill01Atk();
            ListeningClickPlayerSkill02Atk();
            ListeningClickPlayerSkill03Atk();
        }
    }

    #region RegEvts
    //注册触摸事件
    public void ListeningTouchEvts()
    {
        BattleSys.Instance.SetPlayerMoveDir(playerInput.move);
    }

    //释放技能
    public void ListeningClickPlayerNormalAtk()
    {

    }

    public void ListeningClickPlayerSkill01Atk()
    {
        if (playerInput.skill01)
        {
            BattleSys.Instance.ReqPlayerReleaseSkill(1);
        }
    }

    public void ListeningClickPlayerSkill02Atk()
    {

    }

    public void ListeningClickPlayerSkill03Atk()
    {

    }
    #endregion

    public void RefreshUI()
    {
        PlayerData pd = GameRoot.Instance.PlayerData;

        SetText(txtLevel, pd.lv);
        SetText(txtName, pd.name);

        #region Expprg
        int expPrgVal = (int)(pd.exp * 1.0f / PECommon.GetExpUpValByLv(pd.lv) * 100);
        //经验条进度的显示
        SetText(txtExpPrg, expPrgVal + "%");

        int expPrgindex = expPrgVal / 10;

        GridLayoutGroup expGrid = expPrgTrans.GetComponent<GridLayoutGroup>();

        //通过 标准屏幕高度/实际设备屏幕高度，计算出当前UI相对于当前屏幕需要缩放的比例（注意Canvas Scaler 也要基于高度作为缩放标准）
        float globalRate = 1.0f * Constants.ScreenStandardHeight / Screen.height;
        //算出屏幕真实宽度
        float screenWidth = Screen.width * globalRate;
        //减去小的间隙
        float expCellWidth = (screenWidth - 180) / 10;

        expGrid.cellSize = new Vector2(expCellWidth, 7);

        //遍历所有expItem
        for (int i = 0; i < expPrgTrans.childCount; i++)
        {
            Image img = expPrgTrans.GetChild(i).GetComponent<Image>();
            if (i < expPrgindex)
            {
                img.fillAmount = 1;
            }
            else if (i == expPrgindex)
            {
                img.fillAmount = expPrgVal % 10 * 1.0f / 10;
            }
            else
            {
                img.fillAmount = 0;
            }
        }
        #endregion
    }
}
