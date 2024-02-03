//功能：音频播放服务
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioSvc : MonoBehaviour
{
    public static AudioSvc Instance = null;

    public bool _isTurnOnAudio = true;
    public AudioSource bgAudio;
    public AudioSource uiAudio;

    private string bgAudioPath = Constants.bgAudioPath;

    public void InitSvc()
    {
        Instance = this;
        PECommon.Log("Init AudioSvc...");
    }


    public void PlayBGMusic(string name, bool isLoop = true)
    {
        if (!_isTurnOnAudio) { return; }
        AudioClip audio = ResSvc.Instance.LoadAudio(bgAudioPath + name, true);
        if(bgAudio.clip == null || bgAudio.clip.name != audio.name)
        {
            bgAudio.clip = audio;
            bgAudio.loop = isLoop;
            bgAudio.Play();
        }
    }

    public void PlayUIAudio(string name)
    {
        if (!_isTurnOnAudio) { return; }
        AudioClip audio = ResSvc.Instance.LoadAudio(bgAudioPath + name, true);
            uiAudio.clip = audio;
            uiAudio.Play();
    }
}
