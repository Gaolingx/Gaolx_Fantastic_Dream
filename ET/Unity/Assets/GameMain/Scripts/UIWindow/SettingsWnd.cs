using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class SettingsWnd : WindowRoot
{
    public Slider BGAudioSlider, UIAudioSlider, CharacterAudioSlider;
    public Toggle VsyncSettingsToggle, FpsWndToggle, RuntimeInspectorToggle, RuntimeHierarchyToggle;
    public AudioSource WndBGAudioAudioSource, WndUIAudioAudioSource, WndCharacterAudioSource;
    public UIController uiController;
    public Transform DebugItem;
    public Transform fpsWnd;
    public Transform RuntimeHierarchy, RuntimeInspector;

    protected override void InitWnd()
    {
        base.InitWnd();

        GetAudioSourceComponent();
        SliderAddListener();
        InitSliderValue();
    }

    private void InitSliderValue()
    {
        BGAudioSlider.value = WndBGAudioAudioSource.volume;
        UIAudioSlider.value = WndUIAudioAudioSource.volume;
        CharacterAudioSlider.value = WndCharacterAudioSource.volume; 
    }
    private void SliderAddListener()
    {
        BGAudioSlider.onValueChanged.AddListener(TouchBGAudioSlider);
        UIAudioSlider.onValueChanged.AddListener(TouchUIAudioSlider);
        CharacterAudioSlider.onValueChanged.AddListener(TouchCharacterAudioSlider);
    }
    
    public void GetAudioSourceComponent()
    {
        WndBGAudioAudioSource = audioSvc.BGAudioAudioSource;
        WndUIAudioAudioSource = audioSvc.UIAudioAudioSource;
        WndCharacterAudioSource = audioSvc.CharacterAudioSource;
    }

    public void ActiveDebugItemWnd(bool active = true)
    {
        DebugItem.gameObject.SetActive(active);
    }

    public void TouchBGAudioSlider(float volume)
    {
        audioSvc.BGAudioVolumeValue = volume;
        WndBGAudioAudioSource.volume = volume;
    }

    public void TouchUIAudioSlider(float volume)
    {
        audioSvc.UIAudioVolumeValue = volume;
        WndUIAudioAudioSource.volume = volume;
    }

    public void TouchCharacterAudioSlider(float volume)
    {
        audioSvc.CharacterAudioVolumeValue = volume;
        WndCharacterAudioSource.volume = volume;
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
        GameRoot.Instance.GetEventSystemObject(Constants.EventSystemGOName).GetComponent<UIController>().isPause = false;
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
