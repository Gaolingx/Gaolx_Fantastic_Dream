//功能：游戏启动入口，初始化各个业务系统

using PEProtocol;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;
using Newtonsoft.Json;

namespace DarkGod.Main
{
    public class GameRoot : Singleton<GameRoot>
    {
        public bool isDontDestroyOnLoad = true;

        public GameState GameRootGameState { get; set; } = GameState.None;
        public LoadingWnd loadingWnd { get; private set; }
        public DynamicWnd dynamicWnd { get; private set; }
        public SettingsWnd settingsWnd { get; private set; }
        public BattleEndWnd battleEndWnd { get; private set; }
        public StarterAssetsInputs starterAssetsInputs { get; private set; }
        public UICanvasControllerInput uICanvasController { get; private set; }
        public System.Action<bool> SettingsWndAction { get; private set; }
        public System.Action<bool> PauseGameUIAction { get; private set; }
        public System.Action<bool, FBEndType> BattleEndWndAction { get; private set; }

        private bool _isGamePause = false;
        private bool _isInputEnable = true;

        private const string prefsKey_SettingsGameRoot = "prefsKey_SettingsGameRoot";

        [System.Serializable]
        private class PlayerPrefsData
        {
            public int QualityLevel;
        }

        private void LoadPrefsData()
        {
            if (PlayerPrefsSvc.MainInstance.CheckPlayerPrefsHasKey(prefsKey_SettingsGameRoot))
            {
                var json = PlayerPrefsSvc.MainInstance.LoadFromPlayerPrefs(prefsKey_SettingsGameRoot);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData>(json);

                EventMgr.MainInstance.QualityLevel.Value = saveData.QualityLevel;
            }
        }

        private void SavePrefsData(int val)
        {
            var saveData = new PlayerPrefsData();

            saveData.QualityLevel = val;
            PlayerPrefsSvc.MainInstance.SaveByPlayerPrefs(prefsKey_SettingsGameRoot, saveData);
        }

        private void InitTransform()
        {
            loadingWnd = transform.Find(Constants.Path_LoadingWnd_Obj).GetComponent<LoadingWnd>();
            dynamicWnd = transform.Find(Constants.Path_DynamicWnd_Obj).GetComponent<DynamicWnd>();
            settingsWnd = transform.Find(Constants.Path_SettingsWnd_Obj).GetComponent<SettingsWnd>();
            battleEndWnd = transform.Find(Constants.Path_BattleEndWnd_Obj).GetComponent<BattleEndWnd>();
            starterAssetsInputs = transform.Find(Constants.Path_PlayerInputs_Obj).GetComponent<StarterAssetsInputs>();
            uICanvasController = transform.Find(Constants.Path_Joysticks_Obj).GetComponent<UICanvasControllerInput>();
        }

        protected override void Awake()
        {
            base.Awake();

            InitTransform();

            EventMgr.MainInstance.OnGameExit += delegate { GetUIController().OnClickExit(); };
            EventMgr.MainInstance.OnGamePause.OnValueChanged += delegate (bool val) { OnUpdatePauseState(val); };
            EventMgr.MainInstance.QualityLevel.OnValueChanged += delegate (int val) { OnUpdateQualityLevel(val); };
            SettingsWndAction += delegate (bool val) { OpenSettingsWnd(val); };
            PauseGameUIAction += delegate (bool val) { OnPauseGameHandle(val); };
            BattleEndWndAction += delegate (bool val1, FBEndType val2) { OnBattleEndWndHandle(val1, val2); };
        }

        private void Start()
        {
            if (isDontDestroyOnLoad)
            {
                //我们不希望GameRoot及其子物体在切换场景时被销毁
                DontDestroyOnLoad(this);
            }

            EventMgr.MainInstance.SendMessage_GameState(this, new GameStateEventArgs(GameStateEventCode.GameStart));

            CleanUIRoot();
            LoadPrefsData();
            InitGameRoot();

            PECommon.Log("Game Start...");
        }

        private void Update()
        {
            RefreshInputsState();
        }

        private void OpenSettingsWnd(bool state = true)
        {
            if (settingsWnd != null)
            {
                if (settingsWnd.GetWndState() == false)
                {
                    AudioSvc.MainInstance.PlayUIAudio(Constants.UIClickBtn);
                    settingsWnd.SetWndState(state);
                }
            }
        }

        public void OnPauseGameHandle(bool isPause)
        {
            if (isPause)
            {
                EventMgr.MainInstance.SendMessage_GameState(this, new(GameStateEventCode.GamePause));
            }
            else
            {
                EventMgr.MainInstance.SendMessage_GameState(this, new(GameStateEventCode.GameContinue));
            }
        }

        public void OnBattleEndWndHandle(bool isActive, FBEndType endType)
        {
            if (battleEndWnd != null)
            {
                battleEndWnd.SetWndType(endType);
                battleEndWnd.SetWndState(isActive);
            }
        }

        private void OnUpdatePauseState(bool isPause)
        {
            _isGamePause = isPause;

            if (GameRootGameState == GameState.MainCity)
            {

            }
            else if (GameRootGameState == GameState.FBFight)
            {
                BattleSys.MainInstance.battleMgr.SetPauseGame(isPause);
                if (isPause && !settingsWnd.GetWndState())
                {
                    BattleEndWndAction?.Invoke(true, FBEndType.Pause);
                }
            }
        }

        private void OnUpdateQualityLevel(int value)
        {
            QualitySettings.SetQualityLevel(value);
            SavePrefsData(value);
        }

        private void SetCursorLockMode(bool locked)
        {
            GetUIController().CursorLock = locked;
        }

        private void RefreshInputsState()
        {
            if (starterAssetsInputs != null)
            {
                if (!_isGamePause && !starterAssetsInputs.cursorLocked)
                {
                    starterAssetsInputs.canLook = true;
                    SetCursorLockMode(true);
                }
                else
                {
                    starterAssetsInputs.canLook = false;
                    SetCursorLockMode(false);
                }

                if (_isInputEnable && !_isGamePause)
                {
                    starterAssetsInputs.canMove = true;
                }
                else
                {
                    starterAssetsInputs.canMove = false;
                }

                if (starterAssetsInputs.isPause)
                {
                    if (GameRootGameState == GameState.MainCity || GameRootGameState == GameState.Login)
                    {
                        SettingsWndAction?.Invoke(true);
                    }

                    PauseGameUIAction?.Invoke(true);
                }
            }
        }

        private void CleanUIRoot()
        {
            Transform canvas = transform.Find(Constants.Path_Canvas_Obj);

            if (canvas != null)
            {
                for (int i = 0; i < canvas.childCount; i++)
                {
                    canvas.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        //初始化各个系统和服务模块
        private void InitGameRoot()
        {
            if (dynamicWnd != null)
            {
                dynamicWnd.SetWndState();
            }

            //进入登录场景并加载相应UI
            LoginSys.MainInstance.EnterLogin();
        }

        public string GetHotfixVersion()
        {
            return Constants.HotfixBuildVersion;
        }

        public void EnableInputAction(bool state)
        {
            _isInputEnable = state;
        }

        public void ExitGame()
        {
            EventMgr.MainInstance.SendMessage_GameState(this, new GameStateEventArgs(GameStateEventCode.GameStop));
        }

        public UIController GetUIController()
        {
            //return GameObject.Find(Constants.UIControllerRootName).GetComponent<UIController>();
            return UIController.Instance;
        }


        public PlayerData PlayerData { get; private set; }

        public void SetPlayerData(RspLogin data)
        {
            PlayerData = data.playerData;
        }

        public void SetPlayerName(string name)
        {
            PlayerData.name = name;
        }

        public void SetPlayerDataByGuide(RspGuide data)
        {
            PlayerData.coin = data.coin;
            PlayerData.lv = data.lv;
            PlayerData.exp = data.exp;
            PlayerData.guideid = data.guideid;
        }

        public void SetPlayerDataByStrong(RspStrong data)
        {
            PlayerData.coin = data.coin;
            PlayerData.crystal = data.crystal;
            PlayerData.hp = data.hp;
            PlayerData.ad = data.ad;
            PlayerData.ap = data.ap;
            PlayerData.addef = data.addef;
            PlayerData.apdef = data.apdef;

            PlayerData.strongArr = data.strongArr;
        }

        public void SetPlayerDataByBuy(RspBuy data)
        {
            PlayerData.diamond = data.diamond;
            PlayerData.coin = data.coin;
            PlayerData.power = data.power;
        }

        public void SetPlayerDataByPower(PshPower data)
        {
            PlayerData.power = data.power;
        }

        public void SetPlayerDataByTask(RspTakeTaskReward data)
        {
            PlayerData.coin = data.coin;
            PlayerData.lv = data.lv;
            PlayerData.exp = data.exp;
            PlayerData.taskArr = data.taskArr;
        }

        public void SetPlayerDataByTaskPsh(PshTaskPrgs data)
        {
            PlayerData.taskArr = data.taskArr;
        }

        public void SetPlayerDataByFBStart(RspFBFight data)
        {
            PlayerData.power = data.power;
        }

        public void SetPlayerDataByFBEnd(RspFBFightEnd data)
        {
            PlayerData.coin = data.coin;
            PlayerData.lv = data.lv;
            PlayerData.exp = data.exp;
            PlayerData.crystal = data.crystal;
            PlayerData.fuben = data.fuben;
        }


        private void OnDisable()
        {
            EventMgr.MainInstance.OnGameExit -= delegate { GetUIController().OnClickExit(); };
            EventMgr.MainInstance.OnGamePause.OnValueChanged -= delegate (bool val) { OnUpdatePauseState(val); };
            EventMgr.MainInstance.QualityLevel.OnValueChanged -= delegate (int val) { OnUpdateQualityLevel(val); };
            SettingsWndAction -= delegate (bool val) { OpenSettingsWnd(val); };
            PauseGameUIAction -= delegate (bool val) { OnPauseGameHandle(val); };
            BattleEndWndAction -= delegate (bool val1, FBEndType val2) { OnBattleEndWndHandle(val1, val2); };
        }

    }
}
