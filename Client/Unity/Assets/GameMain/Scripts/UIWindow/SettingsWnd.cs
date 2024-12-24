using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DarkGod.Main.QualitySvc;

namespace DarkGod.Main
{
    public class SettingsWnd : WindowRoot, IWindowRoot
    {
        public Slider BGAudioSlider;
        public Slider UIAudioSlider;
        public Slider CharacterAudioSlider;
        public Slider CharacterFxAudioSlider;
        public TMP_Dropdown TargetFrameDropdown;
        public TMP_Dropdown qualitySelectDropdown;
        public TMP_Dropdown screenResolutionDropdown;
        public Toggle FullScreenToggle;
        public Toggle MutedToggle;
        public Button btnResetCfgs;
        public Button btnExitGame;
        public Button btnMainMenu;
        public Button btnCloseSettings;

        private DebugWnd debugWnd;

        private PlayerPrefsData screen = new PlayerPrefsData();

        protected override void InitWnd()
        {
            base.InitWnd();

            screen = QualitySvc.MainInstance.GetScreenSetting();

            if (GameRoot.MainInstance.transform.Find($"{Constants.Path_Canvas_Obj}/DebugItems").TryGetComponent(out debugWnd))
            {
                debugWnd.SetWndState(true);
            }

            InitWindowValue();
        }

        public void OnEnable()
        {
            UIAddListener();
            InputMgr.MainInstance.PauseGameUIAction?.Invoke(true);
        }

        private void InitWindowValue()
        {
            InitDropdownOptionData(TargetFrameDropdown, new List<string>(new string[] { "60", "120", "No Limits" }));
            InitDropdownOptionData(qualitySelectDropdown, new List<string>(new string[] { nameof(GraphicsType.Low), nameof(GraphicsType.Middle), nameof(GraphicsType.High), nameof(GraphicsType.Highest), nameof(GraphicsType.Ultra) }));
            InitDropdownOptionData(screenResolutionDropdown, new List<string>(new string[] { "1024x768", "1280x720", "1360x768", "1600x900", "1920x1080" }));
            qualitySelectDropdown.value = QualitySettings.GetQualityLevel();
            FullScreenToggle.isOn = Screen.fullScreen;
            BGAudioSlider.value = audioSvc.volume.BGAudioVolumeValue.Value;
            UIAudioSlider.value = audioSvc.volume.UIAudioVolumeValue.Value;
            CharacterAudioSlider.value = audioSvc.volume.CharacterAudioVolumeValue.Value;
            CharacterFxAudioSlider.value = audioSvc.volume.CharacterFxAudioVolumeValue.Value;
        }

        private void SendMessageToEventMgr()
        {
            PlayerPrefsData data = new PlayerPrefsData();
            data.graphicsType = screen.graphicsType;
            data.targetFrameRate = screen.targetFrameRate;
            data.resolution = screen.resolution;
            data.fullScreenMode = screen.fullScreenMode;

            EventMgr.OnQualityLevelEvent.SendEventMessage(data);
        }

        #region Slider相关
        private void UIAddListener()
        {
            btnResetCfgs.onClick.AddListener(delegate { ClickResetCfgsBtn(); });
            btnExitGame.onClick.AddListener(delegate { ClickExitGame(); });
            btnMainMenu.onClick.AddListener(delegate { ExitCurrentBattle(); });
            btnCloseSettings.onClick.AddListener(delegate { ClickCloseBtn(); });

            BGAudioSlider.onValueChanged.AddListener(delegate (float val) { TouchBGAudioSlider(val); });
            UIAudioSlider.onValueChanged.AddListener(delegate (float val) { TouchUIAudioSlider(val); });
            CharacterAudioSlider.onValueChanged.AddListener(delegate (float val) { TouchCharacterAudioSlider(val); });
            CharacterFxAudioSlider.onValueChanged.AddListener(delegate (float val) { TouchCharacterFxAudioSlider(val); });

            MutedToggle.onValueChanged.AddListener(delegate (bool val) { ClickMutedToggle(val); });
            FullScreenToggle.onValueChanged.AddListener(delegate (bool val) { ClickFullScreenToggle(val); });

            TargetFrameDropdown.onValueChanged.AddListener(delegate (int val) { OnTargetFrameDropdownValueChanged(val); });
            qualitySelectDropdown.onValueChanged.AddListener(delegate (int val) { OnQualityDropdownValueChanged(val); });
            screenResolutionDropdown.onValueChanged.AddListener(delegate (int val) { OnSetScreenResolution(val); });
        }

        public void TouchBGAudioSlider(float volume)
        {
            audioSvc.volume.BGAudioVolumeValue.Value = volume;
        }

        public void TouchUIAudioSlider(float volume)
        {
            audioSvc.volume.UIAudioVolumeValue.Value = volume;
        }

        public void TouchCharacterAudioSlider(float volume)
        {
            audioSvc.volume.CharacterAudioVolumeValue.Value = volume;
        }

        public void TouchCharacterFxAudioSlider(float volume)
        {
            audioSvc.volume.CharacterFxAudioVolumeValue.Value = volume;
        }

        #endregion

        #region Toggle相关

        public void ClickFullScreenToggle(bool state)
        {
            if (state == true)
            {
                screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }
            else
            {
                screen.fullScreenMode = FullScreenMode.Windowed;
            }
            SendMessageToEventMgr();
        }

        public void ClickMutedToggle(bool val)
        {
            audioSvc.SetMainAudioMuted(val);
        }

        #endregion

        #region Button相关

        public void ClickCloseBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            SetWndState(false);
        }

        #endregion

        #region QualityDropdown

        private void SetQualityLevel(int desiredQualityLevelIndex)
        {
            if (desiredQualityLevelIndex < QualitySettings.names.Length)
            {
                switch (desiredQualityLevelIndex)
                {
                    case 0:
                        screen.graphicsType = GraphicsType.Low;
                        break;
                    case 1:
                        screen.graphicsType = GraphicsType.Middle;
                        break;
                    case 2:
                        screen.graphicsType = GraphicsType.High;
                        break;
                    case 3:
                        screen.graphicsType = GraphicsType.Highest;
                        break;
                    case 4:
                        screen.graphicsType = GraphicsType.Ultra;
                        break;
                    default:
                        break;
                }
                SendMessageToEventMgr();
            }
        }

        private void OnQualityDropdownValueChanged(int value)
        {
            SetQualityLevel(value);
        }

        private void OnSetScreenResolution(int index)
        {
            switch (index)
            {
                case 0:
                    screen.resolution = (1024, 768);
                    break;
                case 1:
                    screen.resolution = (1280, 720);
                    break;
                case 2:
                    screen.resolution = (1360, 768);
                    break;
                case 3:
                    screen.resolution = (1600, 900);
                    break;
                case 4:
                    screen.resolution = (1920, 1080);
                    break;
                default:
                    break;
            }
            SendMessageToEventMgr();
        }

        private void OnTargetFrameDropdownValueChanged(int index)
        {
            switch (index)
            {
                case 0:
                    screen.targetFrameRate = 60;
                    break;
                case 1:
                    screen.targetFrameRate = 120;
                    break;
                case 2:
                    screen.targetFrameRate = -1;
                    break;
                default:
                    break;
            }
            SendMessageToEventMgr();
        }

        #endregion

        public void OnDisable()
        {
            btnResetCfgs.onClick.RemoveAllListeners();
            btnExitGame.onClick.RemoveAllListeners();
            btnMainMenu.onClick.RemoveAllListeners();
            btnCloseSettings.onClick.RemoveAllListeners();

            BGAudioSlider.onValueChanged.RemoveAllListeners();
            UIAudioSlider.onValueChanged.RemoveAllListeners();
            CharacterAudioSlider.onValueChanged.RemoveAllListeners();
            CharacterFxAudioSlider.onValueChanged.RemoveAllListeners();

            MutedToggle.onValueChanged.RemoveAllListeners();
            FullScreenToggle.onValueChanged.RemoveAllListeners();

            TargetFrameDropdown.onValueChanged.RemoveAllListeners();
            qualitySelectDropdown.onValueChanged.RemoveAllListeners();
            screenResolutionDropdown.onValueChanged.RemoveAllListeners();

            InputMgr.MainInstance.PauseGameUIAction?.Invoke(false);
        }
    }
}
