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

        protected override void InitWnd()
        {
            base.InitWnd();

            InitSliderValue();
            SliderAddListener();
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
            if (BattleSys.Instance != null)
            {
                BattleSys.Instance.battleMgr.SetPauseGame(false);
            }
            else
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
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ClickVsyncBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            if (VsyncSettingsToggle.isOn == true)
            {
                QualitySettings.vSyncCount = 1;
                Debug.Log("已开启垂直同步！");
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Debug.Log("已关闭垂直同步！");
            }
        }

        //Reload Cfg Data
        public void ClickResetCfgsBtn()
        {
            resSvc.ResetSkillCfgs();
            GameRoot.AddTips("技能数据重置成功！");
        }
    }
}
