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
    public StarterAssetsInputs playerInput;
    public Transform GamePadTrans;

    private Vector2 currentDir;

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

    private StarterAssetsInputs _playerInput;

    protected override void InitWnd()
    {
        base.InitWnd();

        sk1CDTime = resSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill01).cdTime / 1000.0f;

        RefreshUI();
    }

    private void InitGamepad(StarterAssetsInputs StarterAssetsInputs)
    {
        if (GamePadTrans != null)
        {
            GamePadTrans.gameObject.SetActive(true);
            UICanvasControllerInput uICanvasControllerInput = GamePadTrans.GetComponent<UICanvasControllerInput>();

            uICanvasControllerInput.starterAssetsInputs = StarterAssetsInputs;
        }
    }

    private void Update()
    {
        SetStarterAssetsInputs();

        float delta = Time.deltaTime;

        if (_playerInput != null)
        {
            InitGamepad(_playerInput);

            ListeningTouchEvts();
            ListeningClickPlayerNormalAtk();
            ListeningClickPlayerSkill01Atk();
            ListeningClickPlayerSkill02Atk();
            ListeningClickPlayerSkill03Atk();
            SetCurrentDir();

            UpdateSk1CD(delta);
        }

    }

    private void SetStarterAssetsInputs()
    {
        _playerInput = playerInput;
    }

    #region Skill CD
    private void UpdateSk1CD(float deltaTime)
    {
        _playerInput.skill01 = false;
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
            if (sk1NumCount >= 1)
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
        currentDir = _playerInput.move;
    }

    public Vector2 GetCurrentDir()
    {
        return currentDir;
    }

    #region RegEvts
    //ע�ᴥ���¼�
    public void ListeningTouchEvts()
    {
        if (GameRoot.Instance.GetCurrentPlayer() != null)
        {
            BattleSys.Instance.SetPlayerMoveDir(currentDir);
        }
    }

    //�ͷż���
    public void ListeningClickPlayerNormalAtk()
    {

    }

    public void ListeningClickPlayerSkill01Atk()
    {
        if (_playerInput.skill01)
        {
            SetInputBool(Constants.SkillID_Mar7th00_skill01, true);
            if (isSk1CD == false)
            {
                BattleSys.Instance.ReqPlayerReleaseSkill(1);
                SetInputBool(Constants.SkillID_Mar7th00_skill01, false);
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

    private void SetInputBool(int inputSkillID, bool inputValue)
    {
        if (isSk1CD == false)
        {
            switch (inputSkillID)
            {
                case Constants.SkillID_Mar7th00_skill01:
                    _playerInput.skill01 = inputValue;
                    break;
                default:
                    break;
            }
        }
    }
}
