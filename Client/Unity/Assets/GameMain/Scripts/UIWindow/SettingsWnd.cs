using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class SettingsWnd : WindowRoot, IWindowRoot
    {
        public DebugWnd debugWnd;
        public Slider BGAudioSlider;
        public Slider UIAudioSlider;
        public Slider CharacterAudioSlider;
        public Slider CharacterFxAudioSlider;
        public Dropdown TargetFrameDropdown;
        public Toggle FullScreenToggle;
        public Toggle MutedToggle;
        public Button btnResetCfgs;
        public Button btnExitGame;
        public Button btnMainMenu;
        public Button btnCloseSettings;
        public Dropdown qualitySelectDropdown;

        private UIController _UIController;

        protected override void InitWnd()
        {
            base.InitWnd();

            _UIController = GameRoot.MainInstance.GetUIController();
            InitWindowValue();
            if (debugWnd != null)
            {
                debugWnd.SetWndState(true);
            }
        }

        public void OnEnable()
        {
            UIAddListener();
        }

        private void InitWindowValue()
        {
            InitDropdownOptionData(TargetFrameDropdown, new List<string>(new string[] { "60", "120", "No Limits" }));
            InitDropdownOptionData(qualitySelectDropdown, new List<string>(QualitySettings.names));
            qualitySelectDropdown.value = QualitySettings.GetQualityLevel();
            BGAudioSlider.value = audioSvc.BGAudioVolumeValue.Value;
            UIAudioSlider.value = audioSvc.UIAudioVolumeValue.Value;
            CharacterAudioSlider.value = audioSvc.CharacterAudioVolumeValue.Value;
            CharacterFxAudioSlider.value = audioSvc.CharacterFxAudioVolumeValue.Value;
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
        }

        public void TouchBGAudioSlider(float volume)
        {
            audioSvc.BGAudioVolumeValue.Value = volume;
        }

        public void TouchUIAudioSlider(float volume)
        {
            audioSvc.UIAudioVolumeValue.Value = volume;
        }

        public void TouchCharacterAudioSlider(float volume)
        {
            audioSvc.CharacterAudioVolumeValue.Value = volume;
        }

        public void TouchCharacterFxAudioSlider(float volume)
        {
            audioSvc.CharacterFxAudioVolumeValue.Value = volume;
        }

        #endregion

        #region Toggle相关

        public void ClickFullScreenToggle(bool state)
        {
            _UIController.FullScreen = state;
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
            PauseGameInWnd();
            SetWndState(false);
        }

        #endregion

        #region QualityDropdown

        private void SetQualityLevel(int desiredQualityLevelIndex)
        {
            if (desiredQualityLevelIndex < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(desiredQualityLevelIndex);
                GameRoot.MainInstance.qualityLevel.Value = desiredQualityLevelIndex;
            }
        }

        public void OnQualityDropdownValueChanged(int value)
        {
            SetQualityLevel(value);
        }

        public void OnTargetFrameDropdownValueChanged(int value)
        {
            switch (value)
            {
                case 0:
                    _UIController.FrameRate = 60;
                    break;
                case 1:
                    _UIController.FrameRate = 120;
                    break;
                case 2:
                    _UIController.FrameRate = -1;
                    break;
                default:
                    break;
            }
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
        }

    }
}
