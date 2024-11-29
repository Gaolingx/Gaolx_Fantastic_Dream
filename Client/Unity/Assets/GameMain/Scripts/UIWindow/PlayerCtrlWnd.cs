//功能：玩家控制界面

using DarkGod.Tools;
using PEProtocol;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class PlayerCtrlWnd : WindowRoot, IWindowRoot
    {
        public TMP_Text txtLevel;
        public TMP_Text txtName;
        public TMP_Text txtExpPrg;

        public Image imgHead;

        public Button btnSettings;
        public Button btnNormal;
        public Button btnSkill1;
        public Button btnSkill2;
        public Button btnSkill3;

        public Transform expPrgTrans;
        public bool CanRlsSkill;
        private SettingsWnd settingsWnd;

        private StarterAssetsInputs playerInput;
        private UICanvasControllerInput uICanvasController;

        #region Action List
        private BindableProperty<Vector2> InputMoveDir { get; set; } = new BindableProperty<Vector2>();
        private BindableProperty<bool> InputPlayerNormalAtk { get; set; } = new BindableProperty<bool>();
        private BindableProperty<bool> InputPlayerSkill01 { get; set; } = new BindableProperty<bool>();
        private BindableProperty<bool> InputPlayerSkill02 { get; set; } = new BindableProperty<bool>();
        private BindableProperty<bool> InputPlayerSkill03 { get; set; } = new BindableProperty<bool>();

        #endregion


        #region Skill
        #region SK1
        public Image imgSk1CD;
        public TMP_Text txtSk1CD;
        private bool isSk1CD = false;
        private float sk1CDTime;
        private int sk1Num;
        private float sk1FillCount = 0;
        private float sk1NumCount = 0;
        #endregion

        #region SK2
        public Image imgSk2CD;
        public TMP_Text txtSk2CD;
        private bool isSk2CD = false;
        private float sk2CDTime;
        private int sk2Num;
        private float sk2FillCount = 0;
        private float sk2NumCount = 0;
        #endregion

        #region SK3
        public Image imgSk3CD;
        public TMP_Text txtSk3CD;
        private bool isSk3CD = false;
        private float sk3CDTime;
        private int sk3Num;
        private float sk3FillCount = 0;
        private float sk3NumCount = 0;
        #endregion

        #endregion

        #region HPDefine
        public TMP_Text txtSelfHP;
        public Image imgSelfHP;

        private int HPSum;
        #endregion


        protected override void InitWnd()
        {
            base.InitWnd();

            settingsWnd = GameRoot.MainInstance.transform.Find($"{Constants.Path_Canvas_Obj}/SettingsPanel").GetComponent<SettingsWnd>();

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

            InputMoveDir.OnValueChanged += delegate (Vector2 val) { OnUpdateInputMoveDir(val); };
            InputPlayerNormalAtk.OnValueChanged += delegate (bool val) { OnUpdategClickNormalAtk(val); };
            InputPlayerSkill01.OnValueChanged += delegate (bool val) { OnUpdateClickSkill01(val); };
            InputPlayerSkill02.OnValueChanged += delegate (bool val) { OnUpdateClickSkill02(val); };
            InputPlayerSkill03.OnValueChanged += delegate (bool val) { OnUpdateClickSkill03(val); };
        }


        private void Awake()
        {
            InitPlayerInput();
        }

        private void Update()
        {
            float delta = Time.deltaTime;

            if (playerInput != null)
            {
                InputMoveDir.Value = playerInput.move;
                InputPlayerNormalAtk.Value = playerInput.normalAtk;
                InputPlayerSkill01.Value = playerInput.skill01;
                InputPlayerSkill02.Value = playerInput.skill02;
                InputPlayerSkill03.Value = playerInput.skill03;

            }

            UpdateSk1CD(delta);
            UpdateSk2CD(delta);
            UpdateSk3CD(delta);
            UpdateBossHPBlend();
        }

        private void OnUpdateInputMoveDir(Vector2 val)
        {
            ListeningTouchEvts(val);
        }

        private void OnUpdategClickNormalAtk(bool val)
        {
            ListeningClickPlayerNormalAtk(val);
        }

        private void OnUpdateClickSkill01(bool val)
        {
            ListeningClickPlayerSkill01Atk(val);
        }

        private void OnUpdateClickSkill02(bool val)
        {
            ListeningClickPlayerSkill02Atk(val);
        }

        private void OnUpdateClickSkill03(bool val)
        {
            ListeningClickPlayerSkill03Atk(val);
        }

        private void InitPlayerInput()
        {
            playerInput = GameRoot.MainInstance.starterAssetsInputs;
            uICanvasController = GameRoot.MainInstance.uICanvasController;

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

        #region RegEvts
        //注册触摸事件
        public void ListeningTouchEvts(Vector2 val)
        {
            BattleSys.MainInstance.SetPlayerMoveDir(val);
        }

        public void ClickSettingsBtn()
        {
            if (settingsWnd != null)
            {
                settingsWnd.SetWndState(true);
            }
        }

        //释放技能
        public void ListeningClickPlayerNormalAtk(bool val)
        {
            if (val == true)
            {
                BattleSys.MainInstance.ReqPlayerReleaseSkill(0);
            }
        }

        public void ListeningClickPlayerSkill01Atk(bool val)
        {
            if (val == true)
            {
                if (isSk1CD == false && CanRlsSkill)
                {
                    BattleSys.MainInstance.ReqPlayerReleaseSkill(1);
                    isSk1CD = true;
                    SetActive(imgSk1CD);
                    imgSk1CD.fillAmount = 1;
                    sk1Num = (int)sk1CDTime;
                    SetText(txtSk1CD, sk1Num);
                }
            }
        }

        public void ListeningClickPlayerSkill02Atk(bool val)
        {
            if (val == true)
            {
                if (isSk2CD == false && CanRlsSkill)
                {
                    BattleSys.MainInstance.ReqPlayerReleaseSkill(2);
                    isSk2CD = true;
                    SetActive(imgSk2CD);
                    imgSk2CD.fillAmount = 1;
                    sk2Num = (int)sk2CDTime;
                    SetText(txtSk1CD, sk2Num);
                }
            }
        }

        public void ListeningClickPlayerSkill03Atk(bool val)
        {
            if (val == true)
            {
                if (isSk3CD == false && CanRlsSkill)
                {
                    BattleSys.MainInstance.ReqPlayerReleaseSkill(3);
                    isSk3CD = true;
                    SetActive(imgSk3CD);
                    imgSk3CD.fillAmount = 1;
                    sk3Num = (int)sk3CDTime;
                    SetText(txtSk3CD, sk3Num);
                }
            }
        }
        #endregion

        public void RefreshUI()
        {
            PlayerData pd = GameRoot.MainInstance.PlayerData;

            SetText(txtLevel, pd.lv);
            SetText(txtName, pd.name);

            SetExpprg(pd, txtExpPrg, expPrgTrans);
            SetSprite(imgHead, PathDefine.PlayerHead);
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

            InputMoveDir.OnValueChanged -= delegate (Vector2 val) { OnUpdateInputMoveDir(val); };
            InputPlayerNormalAtk.OnValueChanged -= delegate (bool val) { OnUpdategClickNormalAtk(val); };
            InputPlayerSkill01.OnValueChanged -= delegate (bool val) { OnUpdateClickSkill01(val); };
            InputPlayerSkill02.OnValueChanged -= delegate (bool val) { OnUpdateClickSkill02(val); };
            InputPlayerSkill03.OnValueChanged -= delegate (bool val) { OnUpdateClickSkill03(val); };
        }

        public void ClickCloseBtn()
        {

        }
    }
}
