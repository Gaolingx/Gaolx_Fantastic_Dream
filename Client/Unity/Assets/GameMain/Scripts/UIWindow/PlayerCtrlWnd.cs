//功能：玩家控制界面

using PEProtocol;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class PlayerCtrlWnd : WindowRoot, IWindowRoot
    {
        public Text txtLevel;
        public Text txtName;
        public Text txtExpPrg;

        public Button btnSettings;
        public Button btnNormal;
        public Button btnSkill1;
        public Button btnSkill2;
        public Button btnSkill3;

        public Transform expPrgTrans;
        public SettingsWnd settingsWnd;

        private StarterAssetsInputs playerInput;
        private UICanvasControllerInput uICanvasController;

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

        public void OnEnable()
        {
            btnSettings.onClick.AddListener(delegate { ClickSettingsBtn(); });
            btnNormal.onClick.AddListener(delegate { uICanvasController.VirtualNormalAtkInput(true); });
            btnSkill1.onClick.AddListener(delegate { uICanvasController.VirtualSkill01Input(true); });
            btnSkill2.onClick.AddListener(delegate { uICanvasController.VirtualSkill02Input(true); });
            btnSkill3.onClick.AddListener(delegate { uICanvasController.VirtualSkill03Input(true); });
        }


        private void Awake()
        {
            InitPlayerInput();
        }

        private void Update()
        {
            float delta = Time.deltaTime;

            if (BattleSys.Instance.currentEntityPlayer == null || GameRoot.MainInstance.GetGameState() != GameState.FBFight)
            {
                return;
            }

            if (playerInput != null)
            {
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

        private void InitPlayerInput()
        {
            playerInput = GameRoot.MainInstance.GetStarterAssetsInputs();
            uICanvasController = GameRoot.MainInstance.GetUICanvasControllerInput();

            if (playerInput != null && uICanvasController != null)
            {
                uICanvasController.gameObject.SetActive(true);
                playerInput.gameObject.SetActive(true);
                uICanvasController.starterAssetsInputs = playerInput;
            }
        }

        private void InitSkCDTime()
        {
            sk1CDTime = configSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill01).cdTime / 1000.0f;
            sk2CDTime = configSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill02).cdTime / 1000.0f;
            sk3CDTime = configSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill03).cdTime / 1000.0f;
        }

        #region Skill CD
        private void UpdateSk1CD(float deltaTime)
        {
            if (isSk1CD)
            {
                //遮罩控制
                sk1FillCount += deltaTime;
                if (sk1FillCount >= sk1CDTime)
                {
                    //CD完成
                    isSk1CD = false;
                    SetActive(imgSk1CD, false);
                    sk1FillCount = 0;
                }
                else
                {
                    //更新冷却进度（从1到0）
                    imgSk1CD.fillAmount = 1 - sk1FillCount / sk1CDTime;
                }

                //时间显示
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
            //只在boss血条出现才更新
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
        //注册触摸事件
        public void ListeningTouchEvts()
        {
            BattleSys.Instance.SetPlayerMoveDir(currentDir);
        }

        //暂停控制
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

        //释放技能
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

        public void RefreshUI()
        {
            PlayerData pd = GameRoot.MainInstance.PlayerData;

            SetText(txtLevel, pd.lv);
            SetText(txtName, pd.name);

            SetExpprg(pd, txtExpPrg, expPrgTrans);
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
        public Image imgYellow; //血条渐变遮罩
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
            currentPrg = UIItemUtils.UpdateMixBlend(currentPrg, targetPrg, Constants.AccelerHPSpeed);
        }

        /// <summary>
        /// 设置Boss血条状态
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="prg">血条默认进度</param>
        public void SetBossHPBarState(bool state, float prg = 1)
        {
            SetActive(transBossHPBar, state);
            imgRed.fillAmount = prg;
            imgYellow.fillAmount = prg;
        }

        #endregion

        public void OnDisable()
        {
            btnSettings.onClick.RemoveAllListeners();
            btnNormal.onClick.RemoveAllListeners();
            btnSkill1.onClick.RemoveAllListeners();
            btnSkill2.onClick.RemoveAllListeners();
            btnSkill3.onClick.RemoveAllListeners();
        }

        public void ClickCloseBtn()
        {

        }
    }
}
