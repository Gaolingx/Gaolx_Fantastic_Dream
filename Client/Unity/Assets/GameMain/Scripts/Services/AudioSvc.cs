//���ܣ���Ƶ���ŷ���
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DarkGod.Main
{
    public class AudioSvc : MonoBehaviour
    {
        public static AudioSvc Instance = null;

        public bool _isTurnOnAudio = true;
        [Range(0, 1)] public float BGAudioVolumeValue, UIAudioVolumeValue, CharacterAudioVolumeValue;
        public GameObject BGAudioGameObject, UIAudioGameObject;
        public AudioSource BGAudioAudioSource, UIAudioAudioSource, CharacterAudioSource;
        public AudioClip[] CharacterFootSteps;
        public AudioClip[] CharacterJumpEfforts;
        public AudioClip[] CharacterLanding;

        private string bgAudioPath = PathDefine.bgAudioPath;

        public void InitSvc()
        {
            Instance = this;

            GetAudioSourceComponent();
            GetAudioSourceValueInit();
            PECommon.Log("Init AudioSvc...");
        }


        private void GetAudioSourceComponent()
        {
            BGAudioGameObject = GameObject.Find(Constants.BGAudioGameObjectName);
            BGAudioAudioSource = BGAudioGameObject.GetComponent<AudioSource>();
            UIAudioGameObject = GameObject.Find(Constants.UIAudioGameObjectName);
            UIAudioAudioSource = UIAudioGameObject.GetComponent<AudioSource>();

        }

        public void GetCharacterAudioSourceComponent(GameObject playerGO)
        {
            CharacterAudioSource = playerGO.GetComponent<AudioSource>();
            CharacterAudioSource.volume = CharacterAudioVolumeValue;
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

        public async void PlayBGMusic(string name, bool isLoop = true, bool isCache = true)
        {
            if (!_isTurnOnAudio) { return; }
            string path = bgAudioPath + name;
            AudioClip audioClip = await ResSvc.Instance.LoadAudioClipAsync(path, isCache);
            if (BGAudioAudioSource.clip == null || BGAudioAudioSource.clip.name != audioClip.name)
            {
                BGAudioAudioSource.clip = audioClip;
                BGAudioAudioSource.loop = isLoop;
                BGAudioAudioSource.volume = BGAudioVolumeValue;
                BGAudioAudioSource.Play();
            }
        }

        public async void PlayUIAudio(string name, bool isCache = true)
        {
            if (!_isTurnOnAudio) { return; }
            string path = bgAudioPath + name;
            AudioClip audioClip = await ResSvc.Instance.LoadAudioClipAsync(path, isCache);
            UIAudioAudioSource.clip = audioClip;
            UIAudioAudioSource.volume = UIAudioVolumeValue;
            UIAudioAudioSource.Play();
        }

        public void PlayFootStep()
        {
            int i = Random.Range(0, CharacterFootSteps.Length);
            CharacterAudioSource.PlayOneShot(CharacterFootSteps[i], CharacterAudioVolumeValue);
        }

        public void PlayJumpEffort()
        {
            int i = Random.Range(0, CharacterJumpEfforts.Length);
            CharacterAudioSource.PlayOneShot(CharacterFootSteps[i], CharacterAudioVolumeValue);
        }

        public void PlayLanding()
        {
            int i = Random.Range(0, CharacterLanding.Length);
            CharacterAudioSource.PlayOneShot(CharacterFootSteps[i], CharacterAudioVolumeValue);
        }
    }
}
