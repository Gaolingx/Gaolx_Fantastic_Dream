using System.Collections;
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
        private BattleEndWnd battleEndWnd;

        private UIController _UIController;

        protected override void InitWnd()
        {
            base.InitWnd();

            debugWnd = GameRoot.MainInstance.transform.Find($"{Constants.Path_Canvas_Obj}/DebugItems").gameObject.GetComponent<DebugWnd>();
            battleEndWnd = GameRoot.MainInstance.transform.Find($"{Constants.Path_Canvas_Obj}/BattleEndWnd").gameObject.GetComponent<BattleEndWnd>();

            if (debugWnd != null)
            {
                debugWnd.SetWndState(true);
            }

            _UIController = GameRoot.MainInstance.GetUIController();

            if (battleEndWnd != null)
            {
                battleEndWnd.SetWndState(false);
            }

            InitWindowValue();
        }

        public void OnEnable()
        {
            UIAddListener();
            GameRoot.MainInstance.PauseGameUIAction?.Invoke(true);
        }

        private void InitWindowValue()
        {
            InitDropdownOptionData(TargetFrameDropdown, new List<string>(new string[] { "60", "120", "No Limits" }));
            InitDropdownOptionData(qualitySelectDropdown, new List<string>(QualitySettings.names));
            InitDropdownOptionData(screenResolutionDropdown, new List<string>(new string[] { "1024x768", "1280x720", "1360x768", "1600x900", "1920x1080" }));
            qualitySelectDropdown.value = QualitySettings.GetQualityLevel();
            FullScreenToggle.isOn = Screen.fullScreen;
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
            screenResolutionDropdown.onValueChanged.AddListener(delegate (int val) { OnSetScreenResolution(val); });
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
            SetWndState(false);
        }

        #endregion

        #region QualityDropdown

        private void SetQualityLevel(int desiredQualityLevelIndex)
        {
            if (desiredQualityLevelIndex < QualitySettings.names.Length)
            {
                EventMgr.MainInstance.QualityLevel.Value = desiredQualityLevelIndex;
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
                    _UIController.ScreenResolution = new Vector2(1024, 768);
                    break;
                case 1:
                    _UIController.ScreenResolution = new Vector2(1280, 720);
                    break;
                case 2:
                    _UIController.ScreenResolution = new Vector2(1360, 768);
                    break;
                case 3:
                    _UIController.ScreenResolution = new Vector2(1600, 900);
                    break;
                case 4:
                    _UIController.ScreenResolution = new Vector2(1920, 1080);
                    break;
                default:
                    break;
            }
        }

        private void OnTargetFrameDropdownValueChanged(int index)
        {
            switch (index)
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
            screenResolutionDropdown.onValueChanged.RemoveAllListeners();

            GameRoot.MainInstance.PauseGameUIAction?.Invoke(false);
        }
    }
}
