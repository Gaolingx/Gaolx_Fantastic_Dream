//功能：音频播放服务
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioSvc : MonoBehaviour
{
    public static AudioSvc Instance = null;

    public bool _isTurnOnAudio = true;
    [Range(0, 1)] public float BGAudioVolumeValue, UIAudioVolumeValue;
    public GameObject BGAudioGameObject, UIAudioGameObject;
    public AudioSource BGAudioAudioSource, UIAudioAudioSource;

    private string bgAudioPath = PathDefine.bgAudioPath;

    public void InitSvc()
    {
        Instance = this;

        GetAudioGameObjectComponent();
        GetAudioSourceValueInit();
        PECommon.Log("Init AudioSvc...");
    }


    private void GetAudioGameObjectComponent()
    {
        BGAudioGameObject = GameObject.Find(Constants.BGAudioGameObjectName);
        BGAudioAudioSource = BGAudioGameObject.GetComponent<AudioSource>();
        UIAudioGameObject = GameObject.Find(Constants.UIAudioGameObjectName);
        UIAudioAudioSource = UIAudioGameObject.GetComponent<AudioSource>();

    }

    private void GetAudioSourceValueInit()
    {
        if (BGAudioGameObject != null)
        {
            BGAudioVolumeValue = BGAudioAudioSource.volume;
        }
        if (UIAudioGameObject != null)
        {
            UIAudioVolumeValue = UIAudioAudioSource.volume;
        }
    }

    public void PlayBGMusic(string name, bool isLoop = true)
    {
        if (!_isTurnOnAudio) { return; }
        AudioClip audio = ResSvc.Instance.LoadAudio(bgAudioPath + name, true);
        if(BGAudioAudioSource.clip == null || BGAudioAudioSource.clip.name != audio.name)
        {
            BGAudioAudioSource.clip = audio;
            BGAudioAudioSource.loop = isLoop;
            BGAudioAudioSource.volume = BGAudioVolumeValue;
            BGAudioAudioSource.Play();
        }
    }

    public void PlayUIAudio(string name)
    {
        if (!_isTurnOnAudio) { return; }
        AudioClip audio = ResSvc.Instance.LoadAudio(bgAudioPath + name, true);
        UIAudioAudioSource.clip = audio;
        UIAudioAudioSource.volume = UIAudioVolumeValue;
        UIAudioAudioSource.Play();
    }
}
