using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//功能：音频播放服务
public class AudioSvc : MonoBehaviour
{
    public static AudioSvc Instance = null;
    public AudioSource bgAudio;
    public AudioSource uiAudio;

    public string bgAudioPath = "ResAudio/";

    public void InitSvc()
    {
        Instance = this;
        Debug.Log("Init AudioSvc...");
    }


    public void PlayBGMusic(string name, bool isLoop = true)
    {
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
        AudioClip audio = ResSvc.Instance.LoadAudio(bgAudioPath + name, true);
            uiAudio.clip = audio;
            uiAudio.Play();
    }
}
