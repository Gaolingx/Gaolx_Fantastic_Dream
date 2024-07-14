using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class SettingsWnd : WindowRoot
    {
        public Slider BGAudioSlider, UIAudioSlider, CharacterAudioSlider, CharacterFxAudioSlider;
        public Toggle VsyncSettingsToggle, MutedToggle, FpsWndToggle, RuntimeInspectorToggle, RuntimeHierarchyToggle;
        public Transform DebugItem;
        public Transform fpsWnd;
        public Transform RuntimeHierarchy, RuntimeInspector;
        public Dropdown qualitySelectDropdown;

        protected override void InitWnd()
        {
            base.InitWnd();

            InitQualityDropdownOptionData();
            SliderAddListener();
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
                GameRoot.MainInstance.PauseGameUI(false);
            }
        }

        private void InitSliderValue()
        {
            VsyncSettingsToggle.isOn = GetVSyncCount();
            MutedToggle.isOn = audioSvc.GetAllAudioObjectMuted();
            BGAudioSlider.value = audioSvc.BGAudioVolumeValue;
            UIAudioSlider.value = audioSvc.UIAudioVolumeValue;
            CharacterAudioSlider.value = audioSvc.CharacterAudioVolumeValue;
            CharacterFxAudioSlider.value = audioSvc.CharacterFxAudioVolumeValue;
        }

        #region Slider相关
        private void SliderAddListener()
        {
            BGAudioSlider.onValueChanged.AddListener(TouchBGAudioSlider);
            UIAudioSlider.onValueChanged.AddListener(TouchUIAudioSlider);
            CharacterAudioSlider.onValueChanged.AddListener(TouchCharacterAudioSlider);
            CharacterFxAudioSlider.onValueChanged.AddListener(TouchCharacterFxAudioSlider);
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
        public void ClickVsyncToggle()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            GameRoot.MainInstance.SetVsyncState(VsyncSettingsToggle.isOn);
        }

        public void ClickMutedToggle()
        {
            audioSvc.SetAllAudioObjectMuted(MutedToggle.isOn);
        }

        public void ClickFpsWndToggle()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            ActiveDebugItemWnd();
            fpsWnd.gameObject.SetActive(FpsWndToggle.isOn);
        }

        public void ClickRuntimeHierarchyToggle()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            ActiveDebugItemWnd();
            RuntimeHierarchy.gameObject.SetActive(RuntimeHierarchyToggle.isOn);
        }

        public void ClickRuntimeInspectorToggle()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            ActiveDebugItemWnd();
            RuntimeInspector.gameObject.SetActive(RuntimeInspectorToggle.isOn);
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

        //Reload Cfg Data
        public void ClickResetCfgsBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            resSvc.ResetSkillCfgs();
            GameRoot.AddTips("技能数据重置成功！");
        }

        #endregion

        #region QualityDropdown
        private void InitQualityDropdownOptionData()
        {
            string[] qualityArr = QualitySettings.names;
            List<string> qualityLst = new List<string>(qualityArr);

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

        #endregion

    }
}
