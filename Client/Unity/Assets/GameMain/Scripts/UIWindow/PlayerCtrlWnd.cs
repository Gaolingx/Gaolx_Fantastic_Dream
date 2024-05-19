//功能：玩家控制界面

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

        private StarterAssetsInputs _playerInput;

        protected override void InitWnd()
        {
            base.InitWnd();

            InitSkCDTime();

            RefreshUI();
            InitHPVal();
        }

        private void Update()
        {
            SetStarterAssetsInputs(playerInput);

            float delta = Time.deltaTime;

            if (_playerInput != null)
            {
                InitGamepad(_playerInput);

                SetCurrentDir();

                if (GameRoot.Instance.GetCurrentPlayer() != null)
                {
                    ListeningTouchEvts();
                    ListeningClickPlayerNormalAtk();
                    ListeningClickPlayerSkill01Atk();
                    ListeningClickPlayerSkill02Atk();
                    ListeningClickPlayerSkill03Atk();
                }

                UpdateSk1CD(delta);
                UpdateSk2CD(delta);
                UpdateSk3CD(delta);
            }

        }

        private void InitSkCDTime()
        {
            sk1CDTime = resSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill01).cdTime / 1000.0f;
            sk2CDTime = resSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill02).cdTime / 1000.0f;
            sk3CDTime = resSvc.GetSkillCfg(Constants.SkillID_Mar7th00_skill03).cdTime / 1000.0f;
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

        private void SetStarterAssetsInputs(StarterAssetsInputs inputs)
        {
            _playerInput = inputs;
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

        private void SetCurrentDir()
        {
            currentDir = _playerInput.move;
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

        //释放技能
        public void ListeningClickPlayerNormalAtk()
        {
            if (_playerInput.normalAtk)
            {
                BattleSys.Instance.ReqPlayerReleaseSkill(0);
            }

            _playerInput.normalAtk = false;
        }

        public void ListeningClickPlayerSkill01Atk()
        {
            if (_playerInput.skill01)
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

            _playerInput.skill01 = false;
        }

        public void ListeningClickPlayerSkill02Atk()
        {
            if (_playerInput.skill02)
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

            _playerInput.skill02 = false;
        }

        public void ListeningClickPlayerSkill03Atk()
        {
            if (_playerInput.skill03)
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

            _playerInput.skill03 = false;
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

        #region HPVal
        public void InitHPVal()
        {
            HPSum = GameRoot.Instance.PlayerData.hp;
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
    }
}
