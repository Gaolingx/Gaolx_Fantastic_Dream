//���ܣ���ҿ��ƽ���

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

    private EntityPlayer wndEntitySelfPlayer;

    protected override void InitWnd()
    {
        base.InitWnd();

        battleMgr = BattleMgr.Instance;
        RefreshUI();
    }

    private void Update()
    {
        if(battleMgr.entitySelfPlayer != null)
        {
            ListeningTouchEvts();
        }
    }

    #region RegEvts
    //ע�ᴥ���¼�
    public void ListeningTouchEvts()
    {
        wndEntitySelfPlayer = battleMgr.entitySelfPlayer;
        StarterAssetsInputs playerInput = wndEntitySelfPlayer.playerInput;
        BattleSys.Instance.SetPlayerMoveDir(playerInput.move);
    }

    //�ͷż���
    public void ClickPlayerNormalAtk()
    {
        BattleSys.Instance.ReqPlayerReleaseSkill(0);
    }

    public void ClickPlayerSkill01Atk()
    {
        BattleSys.Instance.ReqPlayerReleaseSkill(1);
    }

    public void ClickPlayerSkill02Atk()
    {
        BattleSys.Instance.ReqPlayerReleaseSkill(2);
    }

    public void ClickPlayerSkill03Atk()
    {
        BattleSys.Instance.ReqPlayerReleaseSkill(3);
    }
    #endregion

    public void RefreshUI()
    {
        PlayerData pd = GameRoot.Instance.PlayerData;

        SetText(txtLevel, pd.lv);
        SetText(txtName, pd.name);

        #region Expprg
        int expPrgVal = (int)(pd.exp * 1.0f / PECommon.GetExpUpValByLv(pd.lv) * 100);
        //���������ȵ���ʾ
        SetText(txtExpPrg, expPrgVal + "%");

        int expPrgindex = expPrgVal / 10;

        GridLayoutGroup expGrid = expPrgTrans.GetComponent<GridLayoutGroup>();

        //ͨ�� ��׼��Ļ�߶�/ʵ���豸��Ļ�߶ȣ��������ǰUI����ڵ�ǰ��Ļ��Ҫ���ŵı�����ע��Canvas Scaler ҲҪ���ڸ߶���Ϊ���ű�׼��
        float globalRate = 1.0f * Constants.ScreenStandardHeight / Screen.height;
        //�����Ļ��ʵ���
        float screenWidth = Screen.width * globalRate;
        //��ȥС�ļ�϶
        float expCellWidth = (screenWidth - 180) / 10;

        expGrid.cellSize = new Vector2(expCellWidth, 7);

        //��������expItem
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
