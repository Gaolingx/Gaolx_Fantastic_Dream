using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWnd : WindowRoot
{
    public Slider BGAudioSlider, UIAudioSlider, CharacterAudioSlider;
    public Toggle VsyncSettingsBtn;
    public AudioSource WndBGAudioAudioSource, WndUIAudioAudioSource, WndCharacterAudioSource;
    public UIController uiController;
    public FpsWnd fpsWnd;
    protected override void InitWnd()
    {
        base.InitWnd();

        GetAudioSourceComponent();
        SliderAddListener();
        InitSliderValue();
        fpsWnd.SetWndState();
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
    public void ClickCloseBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        uiController.isPause = false;
        fpsWnd.SetWndState(false);
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
        if (VsyncSettingsBtn.isOn == true)
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
