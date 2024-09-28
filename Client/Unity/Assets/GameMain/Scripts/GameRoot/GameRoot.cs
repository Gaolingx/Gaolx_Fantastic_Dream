//功能：游戏启动入口，初始化各个业务系统
using PEProtocol;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;
using UnityEngine.SceneManagement;
using System.Linq;

namespace DarkGod.Main
{
    public class GameRoot : Singleton<GameRoot>
    {
        public bool isDontDestroyOnLoad = true;

        public LoadingWnd loadingWnd;
        public DynamicWnd dynamicWnd;

        public StarterAssetsInputs starterAssetsInputs;
        public UICanvasControllerInput uICanvasController;

        public List<string> ShowCursorScene;

        public System.Action OnGameEnter { get; set; }
        public System.Action OnGameExit { get; set; }
        private BindableProperty<bool> pauseState { get; set; } = new BindableProperty<bool>();

        protected override void Awake()
        {
            base.Awake();

            InitStarterAssetsInputs();

            OnGameExit += GetUIController().OnClickExit;
            pauseState.OnValueChanged += OnUpdatePauseState;
        }

        private void Start()
        {
            if (isDontDestroyOnLoad)
            {
                //我们不希望GameRoot及其子物体在切换场景时被销毁
                DontDestroyOnLoad(this);
            }

            OnGameEnter?.Invoke();

            CleanUIRoot();

            QualitySettings.SetQualityLevel((int)PlayerPrefsSvc.MainInstance.GetSettingsItem("Settings_QualitySelect"));
            InitGameRoot();
            PECommon.Log("Game Start...");
        }

        private void Update()
        {
            RefreshInputsState();
        }

        public void PauseGameUI(bool state = true)
        {
            pauseState.Value = state;
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

        private void InitStarterAssetsInputs()
        {
            starterAssetsInputs = transform.Find(Constants.Path_PlayerInputs_Obj).gameObject.GetComponent<StarterAssetsInputs>();
            uICanvasController = transform.Find(Constants.Path_Joysticks_Obj).GetComponent<UICanvasControllerInput>();
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
            if (pauseState.Value == true || starterAssetsInputs.cursorLocked == true || GetCursorLockModeState() == true)
            {
                GetUIController().CursorLock = CursorLockMode.None;
            }
            else
            {
                GetUIController().CursorLock = CursorLockMode.Locked;
            }
        }

        private bool _isInputEnable = true;
        private void RefreshInputsState()
        {
            if (starterAssetsInputs != null)
            {
                SetCursorLockMode();

                if (_isInputEnable && !pauseState.Value && !starterAssetsInputs.cursorLocked)
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
            Transform input = transform.Find(Constants.Path_PlayerInputs_Obj);
            if (canvas != null)
            {
                for (int i = 0; i < canvas.childCount; i++)
                {
                    canvas.GetChild(i).gameObject.SetActive(false);
                }
            }

            if (input != null)
            {
                input.gameObject.SetActive(true);
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
            OnGameExit?.Invoke();
        }

        public UIController GetUIController()
        {
            //return GameObject.Find(Constants.UIControllerRootName).GetComponent<UIController>();
            return UIController.Instance.GetComponent<UIController>();
        }

        public void SetVsyncState(bool state)
        {
            if (state == true)
            {
                GetUIController().FrameRate = 60;
            }
            else
            {
                GetUIController().FrameRate = -1;
            }
        }

        private PlayerData _playerData = null;
        public PlayerData PlayerData { get { return _playerData; } }
        public void SetPlayerData(RspLogin data)
        {
            _playerData = data.playerData;
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
            pauseState.OnValueChanged -= OnUpdatePauseState;
        }

    }
}
