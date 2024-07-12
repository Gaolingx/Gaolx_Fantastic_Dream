//���ܣ���ҿ��ƽ���

using PEProtocol;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class PlayerCtrlWnd : WindowRoot
    {
        public Text txtLevel;
        public Text txtName;
        public Text txtExpPrg;
        public Transform expPrgTrans;
        public Transform GamePadTrans;
        public SettingsWnd settingsWnd;

        private StarterAssetsInputs playerInput;

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

        #region SK2
        public Image imgSk2CD;
        public Text txtSk2CD;
        private bool isSk2CD = false;
        private float sk2CDTime;
        private int sk2Num;
        private float sk2FillCount = 0;
        private float sk2NumCount = 0;
        #endregion

        #region SK3
        public Image imgSk3CD;
        public Text txtSk3CD;
        private bool isSk3CD = false;
        private float sk3CDTime;
        private int sk3Num;
        private float sk3FillCount = 0;
        private float sk3NumCount = 0;
        #endregion

        #endregion

        #region HPDefine
        public Text txtSelfHP;
        public Image imgSelfHP;

        private int HPSum;
        #endregion

        protected override void InitWnd()
        {
            base.InitWnd();

            InitSkCDTime();

            SetBossHPBarState(false);
            RefreshUI();
            InitHPVal();
        }

        private void Update()
        {
            float delta = Time.deltaTime;

            if (BattleSys.Instance.GetCurrentPlayer() == null || GameRoot.MainInstance.GetGameState() != GameState.FBFight)
            {
                return;
            }

            playerInput = GameRoot.MainInstance.GetStarterAssetsInputs();

            if (playerInput != null)
            {
                InitGamepad();

                SetCurrentDir();

                if (!BattleSys.Instance.battleMgr.GetPauseGame())
                {
                    ListeningTouchEvts();
                    ListeningClickGamePause();
                    ListeningClickPlayerNormalAtk();
                    ListeningClickPlayerSkill01Atk();
                    ListeningClickPlayerSkill02Atk();
                    ListeningClickPlayerSkill03Atk();
                }

                UpdateSk1CD(delta);
                UpdateSk2CD(delta);
                UpdateSk3CD(delta);
            }

            UpdateBossHPBlend();
        }

        private void InitSkCDTime()
        {
            sk1CDTime = resSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill01).cdTime / 1000.0f;
            sk2CDTime = resSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill02).cdTime / 1000.0f;
            sk3CDTime = resSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill03).cdTime / 1000.0f;
        }

        private void InitGamepad()
        {
            if (GamePadTrans != null)
            {
                GamePadTrans.gameObject.SetActive(true);
                UICanvasControllerInput uICanvasControllerInput = GamePadTrans.GetComponent<UICanvasControllerInput>();

                uICanvasControllerInput.starterAssetsInputs = playerInput;
            }
        }

        #region Skill CD
        private void UpdateSk1CD(float deltaTime)
        {
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
        private void UpdateSk2CD(float deltaTime)
        {
            if (isSk2CD)
            {
                sk2FillCount += deltaTime;
                if (sk2FillCount >= sk2CDTime)
                {
                    isSk2CD = false;
                    SetActive(imgSk2CD, false);
                    sk2FillCount = 0;
                }
                else
                {
                    imgSk2CD.fillAmount = 1 - sk2FillCount / sk2CDTime;
                }

                sk2NumCount += deltaTime;
                if (sk2NumCount >= 1)
                {
                    sk2NumCount -= 1;
                    sk2Num -= 1;
                    SetText(txtSk2CD, sk2Num);
                }
            }
        }
        private void UpdateSk3CD(float deltaTime)
        {
            if (isSk3CD)
            {
                sk3FillCount += deltaTime;
                if (sk3FillCount >= sk3CDTime)
                {
                    isSk3CD = false;
                    SetActive(imgSk3CD, false);
                    sk3FillCount = 0;
                }
                else
                {
                    imgSk3CD.fillAmount = 1 - sk3FillCount / sk3CDTime;
                }

                sk3NumCount += deltaTime;
                if (sk3NumCount >= 1)
                {
                    sk3NumCount -= 1;
                    sk3Num -= 1;
                    SetText(txtSk3CD, sk3Num);
                }
            }
        }
        #endregion

        private void UpdateBossHPBlend()
        {
            //ֻ��bossѪ�����ֲŸ���
            if (transBossHPBar.gameObject.activeSelf)
            {
                BlendBossHP();
                imgYellow.fillAmount = currentPrg;
            }
        }

        private void SetCurrentDir()
        {
            currentDir = playerInput.move;
        }

        public Vector2 GetCurrentDir()
        {
            return currentDir;
        }

        #region RegEvts
        //ע�ᴥ���¼�
        public void ListeningTouchEvts()
        {
            BattleSys.Instance.SetPlayerMoveDir(currentDir);
        }

        //��ͣ����
        public void ListeningClickGamePause()
        {
            if (playerInput.isPause)
            {
                if (!settingsWnd.isActiveAndEnabled)
                {
                    BattleSys.Instance.battleMgr.SetPauseGame(true, true);
                    BattleSys.Instance.SetBattleEndWndState(FBEndType.Pause);
                }
            }

            //_playerInput.isPause = false;
        }

        public void ClickSettingsBtn()
        {
            BattleSys.Instance.battleMgr.SetPauseGame(true, true);
            settingsWnd.SetWndState(true);
        }

        //�ͷż���
        public void ListeningClickPlayerNormalAtk()
        {
            if (playerInput.normalAtk)
            {
                BattleSys.Instance.ReqPlayerReleaseSkill(0);
            }

            playerInput.normalAtk = false;
        }

        public void ListeningClickPlayerSkill01Atk()
        {
            if (playerInput.skill01)
            {
                if (isSk1CD == false && GetCanRlsSkill())
                {
                    BattleSys.Instance.ReqPlayerReleaseSkill(1);
                    isSk1CD = true;
                    SetActive(imgSk1CD);
                    imgSk1CD.fillAmount = 1;
                    sk1Num = (int)sk1CDTime;
                    SetText(txtSk1CD, sk1Num);
                }
            }

            playerInput.skill01 = false;
        }

        public void ListeningClickPlayerSkill02Atk()
        {
            if (playerInput.skill02)
            {
                if (isSk2CD == false && GetCanRlsSkill())
                {
                    BattleSys.Instance.ReqPlayerReleaseSkill(2);
                    isSk2CD = true;
                    SetActive(imgSk2CD);
                    imgSk2CD.fillAmount = 1;
                    sk2Num = (int)sk2CDTime;
                    SetText(txtSk1CD, sk2Num);
                }
            }

            playerInput.skill02 = false;
        }

        public void ListeningClickPlayerSkill03Atk()
        {
            if (playerInput.skill03)
            {
                if (isSk3CD == false && GetCanRlsSkill())
                {
                    BattleSys.Instance.ReqPlayerReleaseSkill(3);
                    isSk3CD = true;
                    SetActive(imgSk3CD);
                    imgSk3CD.fillAmount = 1;
                    sk3Num = (int)sk3CDTime;
                    SetText(txtSk3CD, sk3Num);
                }
            }

            playerInput.skill03 = false;
        }
        #endregion

        #region Expprg
        private void SetExpprg(PlayerData pd)
        {
            int expPrgVal = (int)(pd.exp * 1.0f / PECommon.GetExpUpValByLv(pd.lv) * 100);
            //���������ȵ���ʾ
            SetText(txtExpPrg, expPrgVal + "%");

            int expPrgindex = expPrgVal / 10;

            GridLayoutGroup expGrid = expPrgTrans.GetComponent<GridLayoutGroup>();

            //ͨ�� ��׼��Ļ�߶�/ʵ���豸��Ļ�߶ȣ��������ǰUI����ڵ�ǰ��Ļ��Ҫ���ŵı�����ע��Canvas Scaler ҲҪ���ڸ߶���Ϊ���ű�׼��
            float globalRate = 1.0f * Constants.ScreenStandardWidth / Screen.width;
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
        }
        #endregion

        public void RefreshUI()
        {
            PlayerData pd = GameRoot.MainInstance.PlayerData;

            SetText(txtLevel, pd.lv);
            SetText(txtName, pd.name);

            SetExpprg(pd);
        }

        #region HPVal
        public void InitHPVal()
        {
            HPSum = GameRoot.MainInstance.PlayerData.hp;
            SetText(txtSelfHP, HPSum + "/" + HPSum);
            imgSelfHP.fillAmount = 1;
        }

        public void SetSelfHPBarVal(int val)
        {
            SetText(txtSelfHP, val + "/" + HPSum);
            imgSelfHP.fillAmount = val * 1.0f / HPSum;
        }
        #endregion

        public bool GetCanRlsSkill()
        {
            return BattleSys.Instance.CanRlsSkill();
        }

        #region BossHPItem
        public Transform transBossHPBar;
        public Image imgRed;
        public Image imgYellow; //Ѫ����������
        private float currentPrg = 1f;
        private float targetPrg = 1f;

        public void SetBossHPBarVal(int oldVal, int newVal, int sumVal)
        {
            currentPrg = oldVal * 1.0f / sumVal;
            targetPrg = newVal * 1.0f / sumVal;
            imgRed.fillAmount = targetPrg;
        }

        private void BlendBossHP()
        {
            UIItemUtils.UpdateMixBlend(currentPrg, targetPrg, Constants.AccelerHPSpeed);
        }

        /// <summary>
        /// ����BossѪ��״̬
        /// </summary>
        /// <param name="state">״̬</param>
        /// <param name="prg">Ѫ��Ĭ�Ͻ���</param>
        public void SetBossHPBarState(bool state, float prg = 1)
        {
            SetActive(transBossHPBar, state);
            imgRed.fillAmount = prg;
            imgYellow.fillAmount = prg;
        }

        #endregion
    }
}
