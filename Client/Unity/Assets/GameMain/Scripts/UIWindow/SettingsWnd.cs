using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        protected override void InitWnd()
        {
            base.InitWnd();

            if (InputMgr.MainInstance.transform.Find($"{Constants.Path_Canvas_Obj}/DebugItems").TryGetComponent<DebugWnd>(out debugWnd))
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
            InitDropdownOptionData(qualitySelectDropdown, new List<string>(new string[] { GraphicsType.Low.ToString(), GraphicsType.Middle.ToString(), GraphicsType.High.ToString(), GraphicsType.Highest.ToString(), GraphicsType.Ultra.ToString() }));
            InitDropdownOptionData(screenResolutionDropdown, new List<string>(new string[] { "1024x768", "1280x720", "1360x768", "1600x900", "1920x1080" }));
            qualitySelectDropdown.value = QualitySettings.GetQualityLevel();
            FullScreenToggle.isOn = Screen.fullScreen;
            BGAudioSlider.value = QualitySvc.MainInstance.volume.BGAudioVolumeValue.Value;
            UIAudioSlider.value = QualitySvc.MainInstance.volume.UIAudioVolumeValue.Value;
            CharacterAudioSlider.value = QualitySvc.MainInstance.volume.CharacterAudioVolumeValue.Value;
            CharacterFxAudioSlider.value = QualitySvc.MainInstance.volume.CharacterFxAudioVolumeValue.Value;
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
            QualitySvc.MainInstance.volume.BGAudioVolumeValue.Value = volume;
        }

        public void TouchUIAudioSlider(float volume)
        {
            QualitySvc.MainInstance.volume.UIAudioVolumeValue.Value = volume;
        }

        public void TouchCharacterAudioSlider(float volume)
        {
            QualitySvc.MainInstance.volume.CharacterAudioVolumeValue.Value = volume;
        }

        public void TouchCharacterFxAudioSlider(float volume)
        {
            QualitySvc.MainInstance.volume.CharacterFxAudioVolumeValue.Value = volume;
        }

        #endregion

        #region Toggle相关

        public void ClickFullScreenToggle(bool state)
        {
            if (state == true)
            {
                QualitySvc.MainInstance.screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }
            else
            {
                QualitySvc.MainInstance.screen.fullScreenMode = FullScreenMode.Windowed;
            }
            EventMgr.OnQualityLevelEvent.SendEventMessage();
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
                        QualitySvc.MainInstance.screen.graphicsType = GraphicsType.Low;
                        break;
                    case 1:
                        QualitySvc.MainInstance.screen.graphicsType = GraphicsType.Middle;
                        break;
                    case 2:
                        QualitySvc.MainInstance.screen.graphicsType = GraphicsType.High;
                        break;
                    case 3:
                        QualitySvc.MainInstance.screen.graphicsType = GraphicsType.Highest;
                        break;
                    case 4:
                        QualitySvc.MainInstance.screen.graphicsType = GraphicsType.Ultra;
                        break;
                    default:
                        break;
                }
                EventMgr.OnQualityLevelEvent.SendEventMessage();
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
                    QualitySvc.MainInstance.screen.resolution = (1024, 768);
                    break;
                case 1:
                    QualitySvc.MainInstance.screen.resolution = (1280, 720);
                    break;
                case 2:
                    QualitySvc.MainInstance.screen.resolution = (1360, 768);
                    break;
                case 3:
                    QualitySvc.MainInstance.screen.resolution = (1600, 900);
                    break;
                case 4:
                    QualitySvc.MainInstance.screen.resolution = (1920, 1080);
                    break;
                default:
                    break;
            }
            EventMgr.OnQualityLevelEvent.SendEventMessage();
        }

        private void OnTargetFrameDropdownValueChanged(int index)
        {
            switch (index)
            {
                case 0:
                    QualitySvc.MainInstance.screen.targetFrameRate = 60;
                    break;
                case 1:
                    QualitySvc.MainInstance.screen.targetFrameRate = 120;
                    break;
                case 2:
                    QualitySvc.MainInstance.screen.targetFrameRate = -1;
                    break;
                default:
                    break;
            }
            EventMgr.OnQualityLevelEvent.SendEventMessage();
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
