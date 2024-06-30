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
        public Toggle VsyncSettingsToggle, FpsWndToggle, RuntimeInspectorToggle, RuntimeHierarchyToggle;
        public Transform DebugItem;
        public Transform fpsWnd;
        public Transform RuntimeHierarchy, RuntimeInspector;
        public Dropdown qualitySelectDropdown;

        protected override void InitWnd()
        {
            base.InitWnd();

            InitSliderValue();
            SliderAddListener();
            InitQualityDropdownOptionData();
        }

        private void InitSliderValue()
        {
            BGAudioSlider.value = audioSvc.BGAudioVolumeValue;
            UIAudioSlider.value = audioSvc.UIAudioVolumeValue;
            CharacterAudioSlider.value = audioSvc.CharacterAudioVolumeValue;
            CharacterFxAudioSlider.value = audioSvc.CharacterFxAudioVolumeValue;
        }

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

        public void ActiveDebugItemWnd(bool active = true)
        {
            DebugItem.gameObject.SetActive(active);
        }

        private void PauseGameInWnd()
        {
            if (GameRoot.Instance.GetGameState() == GameState.FBFight)
            {
                BattleSys.Instance.battleMgr.SetPauseGame(false, false);
            }
            else if (GameRoot.Instance.GetGameState() == GameState.MainCity)
            {
                GameRoot.Instance.PauseGameUI(false);
            }
        }

        public void ClickFpsWndToggle()
        {
            ActiveDebugItemWnd();
            if (FpsWndToggle.isOn == true)
            {
                fpsWnd.gameObject.SetActive(true);
            }
            else
            {
                fpsWnd.gameObject.SetActive(false);
            }
        }

        public void ClickRuntimeHierarchyToggle()
        {
            ActiveDebugItemWnd();
            if (RuntimeHierarchyToggle.isOn == true)
            {
                RuntimeHierarchy.gameObject.SetActive(true);
            }
            else
            {
                RuntimeHierarchy.gameObject.SetActive(false);
            }
        }

        public void ClickRuntimeInspectorToggle()
        {
            ActiveDebugItemWnd();
            if (RuntimeInspectorToggle.isOn == true)
            {
                RuntimeInspector.gameObject.SetActive(true);
            }
            else
            {
                RuntimeInspector.gameObject.SetActive(false);
            }
        }

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
            GameRoot.Instance.ExitGame();
        }

        public void ClickVsyncBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            GameRoot.Instance.SetVsyncState(VsyncSettingsToggle.isOn);
        }

        //Reload Cfg Data
        public void ClickResetCfgsBtn()
        {
            resSvc.ResetSkillCfgs();
            GameRoot.AddTips("技能数据重置成功！");
        }

        //Quality Settings
        public void InitQualityDropdownOptionData()
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
        }

        private void SetQualityLevel(int desiredQualityLevelIndex)
        {
            // 获取当前的质量等级索引  
            int currentQualityLevel = QualitySettings.GetQualityLevel();
            Debug.Log("当前质量等级: " + QualitySettings.names[currentQualityLevel]);
            
            if (desiredQualityLevelIndex < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(desiredQualityLevelIndex);
                Debug.Log("已将质量等级设置为: " + QualitySettings.names[desiredQualityLevelIndex]);
            }
            else
            {
                Debug.LogWarning("请求的质量等级索引超出范围: " + QualitySettings.names[desiredQualityLevelIndex]);
            }
        }

        public void OnQualityDropdownValueChanged(int value)
        {
            SetQualityLevel(value);
        }
    }
}
