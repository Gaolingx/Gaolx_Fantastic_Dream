using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class SettingsWnd : WindowRoot
    {
        public Slider BGAudioSlider, UIAudioSlider, CharacterAudioSlider, CharacterFxAudioSlider;
        public Toggle VsyncSettingsToggle, MutedToggle, FpsWndToggle, RuntimeInspectorToggle, RuntimeHierarchyToggle;
        public Button btnResetCfgs;
        public Button btnExitGame;
        public Button btnMainMenu;
        public Button btnCloseSettings;
        public Button btnCloseDebugItem;
        public Dropdown qualitySelectDropdown;

        public Transform DebugItem;
        public Transform fpsWnd;
        public Transform RuntimeHierarchy, RuntimeInspector;

        protected override void InitWnd()
        {
            base.InitWnd();

            InitQualityDropdownOptionData();
            UIAddListener();
            InitSliderValue();
        }

        private void ActiveDebugItemWnd(bool active = true)
        {
            DebugItem.gameObject.SetActive(active);
        }

        private bool GetVSyncCount()
        {
            if (QualitySettings.vSyncCount == 0)
            {
                return false;
            }
            else if (QualitySettings.vSyncCount == 1)
            {
                return true;
            }
            return false;
        }

        private void PauseGameInWnd()
        {
            if (GameRoot.MainInstance.GetGameState() == GameState.FBFight)
            {
                BattleSys.Instance.battleMgr.SetPauseGame(false, false);
            }
            else if (GameRoot.MainInstance.GetGameState() == GameState.MainCity)
            {
                MainCitySys.Instance.PauseGameLogic(false);
            }
        }

        private void InitSliderValue()
        {
            VsyncSettingsToggle.isOn = GetVSyncCount();
            BGAudioSlider.value = audioSvc.BGAudioVolumeValue;
            UIAudioSlider.value = audioSvc.UIAudioVolumeValue;
            CharacterAudioSlider.value = audioSvc.CharacterAudioVolumeValue;
            CharacterFxAudioSlider.value = audioSvc.CharacterFxAudioVolumeValue;
        }

        #region Slider相关
        private void UIAddListener()
        {
            btnResetCfgs.onClick.AddListener(delegate { ClickResetCfgsBtn(); });
            btnExitGame.onClick.AddListener(delegate { ClickExitGame(); });
            btnMainMenu.onClick.AddListener(delegate { ExitCurrentBattle(); });
            btnCloseSettings.onClick.AddListener(delegate { ClickCloseBtn(); });
            btnCloseDebugItem.onClick.AddListener(delegate { ClickCloseDebugItemBtn(); });

            BGAudioSlider.onValueChanged.AddListener(TouchBGAudioSlider);
            UIAudioSlider.onValueChanged.AddListener(TouchUIAudioSlider);
            CharacterAudioSlider.onValueChanged.AddListener(TouchCharacterAudioSlider);
            CharacterFxAudioSlider.onValueChanged.AddListener(TouchCharacterFxAudioSlider);

            MutedToggle.onValueChanged.AddListener(ClickMutedToggle);
            VsyncSettingsToggle.onValueChanged.AddListener(ClickVsyncToggle);
            FpsWndToggle.onValueChanged.AddListener(ClickFpsWndToggle);
            RuntimeHierarchyToggle.onValueChanged.AddListener(ClickRuntimeHierarchyToggle);
            RuntimeInspectorToggle.onValueChanged.AddListener(ClickRuntimeInspectorToggle);

            qualitySelectDropdown.onValueChanged.AddListener(OnQualityDropdownValueChanged);
        }

        public void TouchBGAudioSlider(float volume)
        {
            audioSvc.BGAudioVolumeValue = volume;
        }

        public void TouchUIAudioSlider(float volume)
        {
            audioSvc.UIAudioVolumeValue = volume;
        }

        public void TouchCharacterAudioSlider(float volume)
        {
            audioSvc.CharacterAudioVolumeValue = volume;
        }

        public void TouchCharacterFxAudioSlider(float volume)
        {
            audioSvc.CharacterFxAudioVolumeValue = volume;
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

        public void ClickFpsWndToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            ActiveDebugItemWnd();
            fpsWnd.gameObject.SetActive(val);
        }

        public void ClickRuntimeHierarchyToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            ActiveDebugItemWnd();
            RuntimeHierarchy.gameObject.SetActive(val);
        }

        public void ClickRuntimeInspectorToggle(bool val)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            ActiveDebugItemWnd();
            RuntimeInspector.gameObject.SetActive(val);
        }

        #endregion

        #region Button相关
        public void ClickCloseDebugItemBtn()
        {
            fpsWnd.gameObject.SetActive(false);
            RuntimeHierarchy.gameObject.SetActive(false);
            RuntimeInspector.gameObject.SetActive(false);
            ActiveDebugItemWnd(false);
        }

        public void ClickCloseBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            PauseGameInWnd();
            SetWndState(false);
        }

        public void ClickExitGame()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            GameRoot.MainInstance.ExitGame();
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
            qualitySelectDropdown.value = GetQualityLevel();
        }

        private int GetQualityLevel()
        {
            // 获取当前的质量等级索引  
            int currentQualityLevel = QualitySettings.GetQualityLevel();
            return currentQualityLevel;
        }

        private void SetQualityLevel(int desiredQualityLevelIndex)
        {
            if (desiredQualityLevelIndex < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(desiredQualityLevelIndex);
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

        private void OnDisable()
        {
            btnResetCfgs.onClick.RemoveAllListeners();
            btnExitGame.onClick.RemoveAllListeners();
            btnMainMenu.onClick.RemoveAllListeners();
            btnCloseSettings.onClick.RemoveAllListeners();
            btnCloseDebugItem.onClick.RemoveAllListeners();

            BGAudioSlider.onValueChanged.RemoveAllListeners();
            UIAudioSlider.onValueChanged.RemoveAllListeners();
            CharacterAudioSlider.onValueChanged.RemoveAllListeners();
            CharacterFxAudioSlider.onValueChanged.RemoveAllListeners();

            MutedToggle.onValueChanged.RemoveAllListeners();
            VsyncSettingsToggle.onValueChanged.RemoveAllListeners();
            FpsWndToggle.onValueChanged.RemoveAllListeners();
            RuntimeHierarchyToggle.onValueChanged.RemoveAllListeners();
            RuntimeInspectorToggle.onValueChanged.RemoveAllListeners();

            qualitySelectDropdown.onValueChanged.RemoveAllListeners();
        }

        #endregion

    }
}
