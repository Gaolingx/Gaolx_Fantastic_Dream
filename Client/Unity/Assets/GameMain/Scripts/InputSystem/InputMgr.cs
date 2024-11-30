using HuHu;
using StarterAssets;

namespace DarkGod.Main
{
    public class InputMgr : Singleton<InputMgr>
    {
        public LoadingWnd loadingWnd { get; private set; }
        public SettingsWnd settingsWnd { get; private set; }
        public BattleEndWnd battleEndWnd { get; private set; }
        public DynamicWnd dynamicWnd { get; private set; }

        public StarterAssetsInputs starterAssetsInputs { get; private set; }
        public UICanvasControllerInput uICanvasController { get; private set; }

        public System.Action<bool> SettingsWndAction { get; private set; }
        public System.Action<bool> PauseGameUIAction { get; private set; }
        public System.Action<bool, FBEndType> BattleEndWndAction { get; private set; }

        private bool _isGamePause = false;
        private bool _isInputEnable = true;

        protected override void Awake()
        {
            base.Awake();

            GameStateEvent.MainInstance.OnGameEnter += delegate { InitMgr(); };
        }

        private void Update()
        {
            RefreshInputsState();
        }

        private void InitTransform()
        {
            loadingWnd = transform.Find(Constants.Path_LoadingWnd_Obj).GetComponent<LoadingWnd>();
            settingsWnd = transform.Find(Constants.Path_SettingsWnd_Obj).GetComponent<SettingsWnd>();
            battleEndWnd = transform.Find(Constants.Path_BattleEndWnd_Obj).GetComponent<BattleEndWnd>();
            dynamicWnd = transform.Find(Constants.Path_DynamicWnd_Obj).GetComponent<DynamicWnd>();

            starterAssetsInputs = transform.Find(Constants.Path_PlayerInputs_Obj).GetComponent<StarterAssetsInputs>();
            uICanvasController = transform.Find(Constants.Path_Joysticks_Obj).GetComponent<UICanvasControllerInput>();
        }

        private void InitMgr()
        {
            InitTransform();
            GameStateEvent.MainInstance.OnGamePause += delegate (bool val) { OnUpdatePauseState(val); };
            SettingsWndAction += delegate (bool val) { OpenSettingsWnd(val); };
            PauseGameUIAction += delegate (bool val) { OnPauseGameHandle(val); };
            BattleEndWndAction += delegate (bool val1, FBEndType val2) { OnBattleEndWndHandle(val1, val2); };
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

        public void OnPauseGameHandle(bool state)
        {
            EventMgr.OnGamePauseEvent.SendEventMessage(state);
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

            if (GameRoot.MainInstance.GameRootGameState == GameState.MainCity)
            {

            }
            else if (GameRoot.MainInstance.GameRootGameState == GameState.FBFight)
            {
                dynamicWnd.SetWndState(!isPause);
                BattleSys.MainInstance.battleMgr.SetPauseGame(isPause);
                if (isPause && !settingsWnd.GetWndState())
                {
                    BattleEndWndAction?.Invoke(true, FBEndType.Pause);
                }
            }
        }

        public void EnableInputAction(bool state)
        {
            _isInputEnable = state;
        }

        private void RefreshInputsState()
        {
            if (starterAssetsInputs != null)
            {
                if (!_isGamePause && !starterAssetsInputs.cursorLocked)
                {
                    starterAssetsInputs.canLook = true;
                    GameRoot.MainInstance.GetUIController().CursorLock = true;
                }
                else
                {
                    starterAssetsInputs.canLook = false;
                    GameRoot.MainInstance.GetUIController().CursorLock = false;
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
                    if (GameRoot.MainInstance.GameRootGameState == GameState.MainCity || GameRoot.MainInstance.GameRootGameState == GameState.Login)
                    {
                        SettingsWndAction?.Invoke(true);
                    }

                    PauseGameUIAction?.Invoke(true);
                }
            }
        }

        private void OnDisable()
        {
            GameStateEvent.MainInstance.OnGameEnter -= delegate { InitMgr(); };
            GameStateEvent.MainInstance.OnGamePause -= delegate (bool val) { OnUpdatePauseState(val); };
            SettingsWndAction -= delegate (bool val) { OpenSettingsWnd(val); };
            PauseGameUIAction -= delegate (bool val) { OnPauseGameHandle(val); };
            BattleEndWndAction -= delegate (bool val1, FBEndType val2) { OnBattleEndWndHandle(val1, val2); };
        }
    }
}
