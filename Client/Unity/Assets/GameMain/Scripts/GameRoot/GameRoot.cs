//���ܣ���Ϸ������ڣ���ʼ������ҵ��ϵͳ
using PEProtocol;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;
using UnityEngine.SceneManagement;
using System.Linq;
using Newtonsoft.Json;

namespace DarkGod.Main
{
    public class GameRoot : Singleton<GameRoot>
    {
        public bool isDontDestroyOnLoad = true;
        public List<string> ShowCursorScene;

        public LoadingWnd loadingWnd { get; set; }
        public DynamicWnd dynamicWnd { get; set; }
        private StarterAssetsInputs starterAssetsInputs;
        private UICanvasControllerInput uICanvasController;

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
            else
            {
                EventMgr.MainInstance.QualityLevel.Value = QualitySettings.GetQualityLevel();
            }
        }

        private void SavePrefsData()
        {
            var saveData = new PlayerPrefsData();

            saveData.QualityLevel = EventMgr.MainInstance.QualityLevel.Value;
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

            EventMgr.MainInstance.OnGameExit += GetUIController().OnClickExit;
            EventMgr.MainInstance.PauseState.OnValueChanged += delegate (bool val) { OnUpdatePauseState(val); };
            EventMgr.MainInstance.QualityLevel.OnValueChanged += delegate (int val) { OnUpdateQualityLevel(val); };
        }

        private void Start()
        {
            if (isDontDestroyOnLoad)
            {
                //���ǲ�ϣ��GameRoot�������������л�����ʱ������
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

        public void PauseGameUI(bool state = true)
        {
            EventMgr.MainInstance.PauseState.Value = state;
        }

        private void OnUpdatePauseState(bool state)
        {
            if (starterAssetsInputs != null)
            {
                starterAssetsInputs.isPause = state;
            }

            if (state == true)
            {
                VFXManager.MainInstance.PauseVFX();
            }
            else
            {
                VFXManager.MainInstance.ResetVFX();
            }
        }

        private void OnUpdateQualityLevel(int value)
        {
            QualitySettings.SetQualityLevel(value);
            SavePrefsData();
        }

        public StarterAssetsInputs GetStarterAssetsInputs()
        {
            return starterAssetsInputs;
        }

        public UICanvasControllerInput GetUICanvasControllerInput()
        {
            return uICanvasController;
        }

        public string GetCurrentSceneName()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            return currentScene.name;
        }

        private bool GetCursorLockModeState()
        {
            return ShowCursorScene.Any(item => item == GetCurrentSceneName());
        }

        private void SetCursorLockMode()
        {
            if (starterAssetsInputs != null)
            {
                if (EventMgr.MainInstance.PauseState.Value == true || starterAssetsInputs.cursorLocked == true || GetCursorLockModeState() == true)
                {
                    GetUIController().CursorLock = CursorLockMode.None;
                }
                else
                {
                    GetUIController().CursorLock = CursorLockMode.Locked;
                }
            }
        }

        private bool _isInputEnable = true;
        private void RefreshInputsState()
        {
            if (starterAssetsInputs != null)
            {
                SetCursorLockMode();

                if (_isInputEnable && !EventMgr.MainInstance.PauseState.Value && !starterAssetsInputs.cursorLocked)
                {
                    starterAssetsInputs.canLook = true;
                }
                else
                {
                    starterAssetsInputs.canLook = false;
                }

                starterAssetsInputs.canMove = _isInputEnable;
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

        //��ʼ������ϵͳ�ͷ���ģ��
        private void InitGameRoot()
        {
            if (dynamicWnd != null)
            {
                dynamicWnd.SetWndState();
            }

            //�����¼������������ӦUI
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

        private GameState gameState = GameState.None;
        public void SetGameState(GameState state)
        {
            gameState = state;
        }
        public GameState GetGameState()
        {
            return gameState;
        }

        private void OnDestroy()
        {
            EventMgr.MainInstance.OnGameExit -= GetUIController().OnClickExit;
            EventMgr.MainInstance.PauseState.OnValueChanged -= delegate (bool val) { OnUpdatePauseState(val); };
            EventMgr.MainInstance.QualityLevel.OnValueChanged -= delegate (int val) { OnUpdateQualityLevel(val); };
        }

    }
}
