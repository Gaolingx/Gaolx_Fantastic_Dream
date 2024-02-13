using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWnd : WindowRoot
{
    public Slider BGAudioSlider, UIAudioSlider;
    public Toggle VsyncSettingsBtn;
    public AudioSource WndBGAudioAudioSource, WndUIAudioAudioSource;
    protected override void InitWnd()
    {
        base.InitWnd();

        GetAudioSourceComponent();
        SliderAddListener();
    }

    private void SliderAddListener()
    {
        BGAudioSlider.onValueChanged.AddListener(TouchBGAudioSlider);
        UIAudioSlider.onValueChanged.AddListener(TouchUIAudioSlider);
    }
    public void GetAudioSourceComponent()
    {
        WndBGAudioAudioSource = audioSvc.BGAudioAudioSource;
        WndUIAudioAudioSource = audioSvc.UIAudioAudioSource;
    }
    public void TouchBGAudioSlider(float volume)
    {
        audioSvc.BGAudioVolumeValue = volume;
        WndBGAudioAudioSource.volume = volume;
    }
    public void TouchUIAudioSlider(float volume)
    {
        audioSvc.BGAudioVolumeValue = volume;
        WndUIAudioAudioSource.volume = volume;
    }
    public void ClickCloseBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        SetWndState(false);
    }
    public void ClickExitGame()
    {
        Application.Quit();
    }
    public void ClickVsyncBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        if (VsyncSettingsBtn.isOn == true)
        {
            QualitySettings.vSyncCount = 1;
            Debug.Log("�ѿ�����ֱͬ����");
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Debug.Log("�ѹرմ�ֱͬ����");
        }
    }
}
