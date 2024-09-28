using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using XiHUtil;

namespace DarkGod.Main
{
    public class SettingsWnd : WindowRoot, IWindowRoot
    {
        public Slider BGAudioSlider, UIAudioSlider, CharacterAudioSlider, CharacterFxAudioSlider;
        public Toggle VsyncSettingsToggle, MutedToggle;
        public Button btnResetCfgs;
        public Button btnExitGame;
        public Button btnMainMenu;
        public Button btnCloseSettings;
        public Dropdown qualitySelectDropdown;

        public enum SettingsPrefsType
        {
            None,
            QualitySelect,
            BGAudioVolume,
            UIAudioVolume,
            CharacterAudioVolume,
            CharacterFxAudioVolume
        }

        protected override void InitWnd()
        {
            base.InitWnd();

            InitWindowValue();
        }

        public void OnEnable()
        {
            UIAddListener();
        }

        private bool GetVSyncCount()
        {
            if (Application.targetFrameRate != -1) { return false; }
            return true;
        }

        private void InitWindowValue()
        {
            VsyncSettingsToggle.isOn = GetVSyncCount();
            BGAudioSlider.value = (float)playerPrefsSvc.GetSettingsItem("Settings_BGAudioSlider");
            UIAudioSlider.value = (float)playerPrefsSvc.GetSettingsItem("Settings_UIAudioSlider");
            CharacterAudioSlider.value = (float)playerPrefsSvc.GetSettingsItem("Settings_CharacterAudioSlider");
            CharacterFxAudioSlider.value = (float)playerPrefsSvc.GetSettingsItem("Settings_CharacterFxAudioSlider");
        }

        #region Slider相关
        private void UIAddListener()
        {
            btnResetCfgs.onClick.AddListener(delegate { ClickResetCfgsBtn(); });
            btnExitGame.onClick.AddListener(delegate { ClickExitGame(); });
            btnMainMenu.onClick.AddListener(delegate { ExitCurrentBattle(); });
            btnCloseSettings.onClick.AddListener(delegate { ClickCloseBtn(); });

            BGAudioSlider.onValueChanged.AddListener(TouchBGAudioSlider);
            UIAudioSlider.onValueChanged.AddListener(TouchUIAudioSlider);
            CharacterAudioSlider.onValueChanged.AddListener(TouchCharacterAudioSlider);
            CharacterFxAudioSlider.onValueChanged.AddListener(TouchCharacterFxAudioSlider);

            MutedToggle.onValueChanged.AddListener(ClickMutedToggle);
            VsyncSettingsToggle.onValueChanged.AddListener(ClickVsyncToggle);

            qualitySelectDropdown.onValueChanged.AddListener(OnQualityDropdownValueChanged);
        }

        public void TouchBGAudioSlider(float volume)
        {
            audioSvc.BGAudioVolumeValue = volume;
            PlayerPrefsUtil.Set("Settings_BGAudioSlider", volume);
        }

        public void TouchUIAudioSlider(float volume)
        {
            audioSvc.UIAudioVolumeValue = volume;
            PlayerPrefsUtil.Set("Settings_UIAudioSlider", volume);
        }

        public void TouchCharacterAudioSlider(float volume)
        {
            audioSvc.CharacterAudioVolumeValue = volume;
            PlayerPrefsUtil.Set("Settings_CharacterAudioSlider", volume);
        }

        public void TouchCharacterFxAudioSlider(float volume)
        {
            audioSvc.CharacterFxAudioVolumeValue = volume;
            PlayerPrefsUtil.Set("Settings_CharacterFxAudioSlider", volume);
        }

        #endregion

        #region Toggle相关
        public void ClickVsyncToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            GameRoot.MainInstance.SetVsyncState(val);
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
        private void InitQualityDropdownOptionData()
        {
            List<string> qualityLst = new List<string>(QualitySettings.names);
            List<Dropdown.OptionData> qualitySelectDropdownOptionData = new List<Dropdown.OptionData>();

            foreach (var item in qualityLst)
            {
                Dropdown.OptionData data = new Dropdown.OptionData();
                data.text = item;
                qualitySelectDropdownOptionData.Add(data);
            }

            qualitySelectDropdown.options = qualitySelectDropdownOptionData;
            qualitySelectDropdown.value = (int)playerPrefsSvc.GetSettingsItem("Settings_QualitySelect");
        }

        private void SetQualityLevel(int desiredQualityLevelIndex)
        {
            if (desiredQualityLevelIndex < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(desiredQualityLevelIndex);
                PlayerPrefsUtil.Set("Settings_QualitySelect", desiredQualityLevelIndex);
            }
            else
            {
                PECommon.Log("请求的质量等级索引超出范围: " + QualitySettings.names[desiredQualityLevelIndex], PELogType.Error);
            }
        }

        public void OnQualityDropdownValueChanged(int value)
        {
            SetQualityLevel(value);
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
            VsyncSettingsToggle.onValueChanged.RemoveAllListeners();

            qualitySelectDropdown.onValueChanged.RemoveAllListeners();
        }

    }
}
