//���ܣ���ҿ��ƽ���

using PEProtocol;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerCtrlWnd : WindowRoot
{
    public Text txtLevel;
    public Text txtName;
    public Text txtExpPrg;
    public Transform expPrgTrans;

    [HideInInspector]
    public Vector2 currentDir;

    #region Skill
    #region SK1
    public Image imgSk1CD;
    public Text txtSk1CD;
    private bool isSk1CD = false;
    private float sk1CDTime;
    private int sk1Num;
    private float sk1FillCount = 0;
    private float sk1NumCount = 0;
    #endregion

    #endregion

    private StarterAssetsInputs playerInput;

    private EntityPlayer entitySelfPlayer;

    protected override void InitWnd()
    {
        base.InitWnd();

        sk1CDTime = resSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill01).cdTime / 1000.0f;

        RefreshUI();
    }

    private void Update()
    {
        entitySelfPlayer = GameRoot.Instance.GetCurrentPlayer();
        if (entitySelfPlayer != null)
        {
            playerInput = entitySelfPlayer.playerInput;

            ListeningTouchEvts();
            ListeningClickPlayerNormalAtk();
            ListeningClickPlayerSkill01Atk();
            ListeningClickPlayerSkill02Atk();
            ListeningClickPlayerSkill03Atk();
            SetCurrentDir();
        }

        float delta = Time.deltaTime;
        UpdateSk1CD(delta);
    }

    #region Skill CD
    private void UpdateSk1CD(float deltaTime)
    {
        playerInput.skill01 = false;
        if (isSk1CD)
        {
            //���ֿ���
            sk1FillCount += deltaTime;
            if (sk1FillCount >= sk1CDTime)
            {
                //CD���
                isSk1CD = false;
                SetActive(imgSk1CD, false);
                sk1FillCount = 0;
            }
            else
            {
                //������ȴ���ȣ���1��0��
                imgSk1CD.fillAmount = 1 - sk1FillCount / sk1CDTime;
            }

            //ʱ����ʾ
            sk1NumCount += deltaTime;
            if(sk1NumCount >= 1)
            {
                sk1NumCount -= 1;
                sk1Num -= 1;
                SetText(txtSk1CD, sk1Num);
            }
        }
    }
    #endregion

    private void SetCurrentDir()
    {
        currentDir = playerInput.move;
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
        if (isSk1CD == false)
        {
            if (playerInput.skill01)
            {
                BattleSys.Instance.ReqPlayerReleaseSkill(1);
                isSk1CD = true;
                SetActive(imgSk1CD);
                imgSk1CD.fillAmount = 1;
                sk1Num = (int)sk1CDTime;
                SetText(txtSk1CD, sk1Num);
            }
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
