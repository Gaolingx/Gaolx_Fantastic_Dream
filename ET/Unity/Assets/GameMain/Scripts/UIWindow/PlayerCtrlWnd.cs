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
    //ע�ᴥ���¼�
    public void ListeningTouchEvts()
    {
        BattleSys.Instance.SetPlayerMoveDir(playerInput.move);
    }

    //�ͷż���
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
