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

        public LoadingWnd loadingWnd { get; set; }
        public DynamicWnd dynamicWnd { get; set; }
        private StarterAssetsInputs starterAssetsInputs;
        private UICanvasControllerInput uICanvasController;

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
            loadingWnd = transform.Find(Constants.Path_LoadingWnd_Obj).gameObject.GetComponent<LoadingWnd>();
            dynamicWnd = transform.Find(Constants.Path_DynamicWnd_Obj).gameObject.GetComponent<DynamicWnd>();
            starterAssetsInputs = transform.Find(Constants.Path_PlayerInputs_Obj).gameObject.GetComponent<StarterAssetsInputs>();
            uICanvasController = transform.Find(Constants.Path_Joysticks_Obj).GetComponent<UICanvasControllerInput>();
        }

        protected override void Awake()
        {
            base.Awake();

            InitTransform();

            EventMgr.MainInstance.OnGameExit += delegate { GetUIController().OnClickExit(); };
            EventMgr.MainInstance.OnGamePause += delegate (bool val) { OnUpdatePauseState(val); };
            EventMgr.MainInstance.QualityLevel.OnValueChanged += delegate (int val) { OnUpdateQualityLevel(val); };
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

        public void PauseGameUI(bool isPause)
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

        private void OnUpdatePauseState(bool isPause)
        {
            _isGamePause = isPause;

            if (GameRootGameState == GameState.MainCity)
            {
                MainCitySys.MainInstance.OpenSettingsWnd();
            }
            else if (GameRootGameState == GameState.FBFight)
            {
                BattleSys.MainInstance.battleMgr.SetPauseGame(true);
                BattleSys.MainInstance.SetBattleEndWndState(FBEndType.Pause);
            }
        }

        private void OnUpdateQualityLevel(int value)
        {
            QualitySettings.SetQualityLevel(value);
            SavePrefsData(value);
        }

        public StarterAssetsInputs GetStarterAssetsInputs()
        {
            return starterAssetsInputs;
        }

        public UICanvasControllerInput GetUICanvasControllerInput()
        {
            return uICanvasController;
        }

        private void SetCursorLockMode(bool locked)
        {
            GetUIController().CursorLock = locked;
        }

        private void RefreshInputsState()
        {
            if (starterAssetsInputs != null)
            {
                if (_isInputEnable && !_isGamePause && !starterAssetsInputs.cursorLocked)
                {
                    starterAssetsInputs.canLook = true;
                    SetCursorLockMode(true);
                }
                else
                {
                    starterAssetsInputs.canLook = false;
                    SetCursorLockMode(false);
                }

                starterAssetsInputs.canMove = _isInputEnable;

                if (starterAssetsInputs.isPause)
                {
                    PauseGameUI(true);
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


        public PlayerData PlayerData { get; set; }

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

        public GameState GameRootGameState { get; set; } = GameState.None;


        private void OnDestroy()
        {
            EventMgr.MainInstance.OnGameExit -= delegate { GetUIController().OnClickExit(); };
            EventMgr.MainInstance.OnGamePause -= delegate (bool val) { OnUpdatePauseState(val); };
            EventMgr.MainInstance.QualityLevel.OnValueChanged -= delegate (int val) { OnUpdateQualityLevel(val); };
        }

    }
}
