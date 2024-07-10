//功能：音频播放服务
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;

namespace DarkGod.Main
{
    public class AudioSvc : Singleton<AudioSvc>
    {
        [Range(0, 1)] public float BGAudioVolumeValue, UIAudioVolumeValue, CharacterAudioVolumeValue, CharacterFxAudioVolumeValue;
        public AudioSource BGAudioAudioSource, UIAudioAudioSource, CharacterAudioSource;
        public string[] FootStepsAudioPaths, JumpEffortsAudioPaths, LandingAudioPaths, HitAudioPaths;

        private AudioClip[] CharacterFootSteps = new AudioClip[10];
        private AudioClip[] CharacterJumpEfforts = new AudioClip[3];
        private AudioClip[] CharacterLanding = new AudioClip[3];
        private AudioClip[] CharacterHit = new AudioClip[3];

        private string bgAudioPath = PathDefine.bgAudioPath;

        private UIController uiController;
        private bool _isTurnOnAudio;

        protected override void Awake()
        {
            base.Awake();
        }

        public void InitSvc()
        {
            InitAudioClipArray(FootStepsAudioPaths, JumpEffortsAudioPaths, LandingAudioPaths, HitAudioPaths);
            uiController = GameRoot.MainInstance.GetUIController();
            PECommon.Log("Init AudioSvc...");
        }

        private void Update()
        {
            RefreshAudioSourceVolume();
            CheckAllAudioObjectMuted();
        }

        private void CheckAllAudioObjectMuted()
        {
            if (uiController == null)
            {
                _isTurnOnAudio = true;
                return;
            }
            _isTurnOnAudio = !uiController._isPause;

            if (!_isTurnOnAudio)
            {
                BGAudioAudioSource.volume = 0f;
                UIAudioAudioSource.volume = 0f;
                if (CharacterAudioSource != null)
                {
                    CharacterAudioSource.volume = 0f;
                }
            }
        }

        public void SetCharacterAudioSource(AudioSource audioSource)
        {
            CharacterAudioSource = audioSource;
            CharacterAudioSource.volume = CharacterAudioVolumeValue;
        }

        public void InitAudioClipArray(string[] name1, string[] name2, string[] name3, string[] name4)
        {
            for (int i = 0; i < name1.Length; i++)
            {
                string path = bgAudioPath + name1[i];
                AudioClip audioClip = ResSvc.MainInstance.LoadAudioClipSync(path);
                CharacterFootSteps[i] = audioClip;
            }

            for (int i = 0; i < name2.Length; i++)
            {
                string path = bgAudioPath + name2[i];
                AudioClip audioClip = ResSvc.MainInstance.LoadAudioClipSync(path);
                CharacterJumpEfforts[i] = audioClip;
            }

            for (int i = 0; i < name3.Length; i++)
            {
                string path = bgAudioPath + name3[i];
                AudioClip audioClip = ResSvc.MainInstance.LoadAudioClipSync(path);
                CharacterLanding[i] = audioClip;
            }

            for (int i = 0; i < name4.Length; i++)
            {
                string path = bgAudioPath + name4[i];
                AudioClip audioClip = ResSvc.MainInstance.LoadAudioClipSync(path);
                CharacterHit[i] = audioClip;
            }
        }

        private void RefreshAudioSourceVolume()
        {
            if (BGAudioAudioSource != null)
            {
                BGAudioAudioSource.volume = BGAudioVolumeValue;
            }
            if (UIAudioAudioSource != null)
            {
                UIAudioAudioSource.volume = UIAudioVolumeValue;
            }
            if (CharacterAudioSource != null)
            {
                CharacterAudioSource.volume = CharacterAudioVolumeValue;
            }
        }

        #region PlayAudio
        public async void PlayBGMusic(string name, bool isLoop = true, bool isCache = true)
        {
            string path = bgAudioPath + name;
            AudioClip audioClip = await ResSvc.MainInstance.LoadAudioClipAsync(path, isCache);
            if (BGAudioAudioSource.clip == null || BGAudioAudioSource.clip.name != audioClip.name)
            {
                BGAudioAudioSource.clip = audioClip;
                BGAudioAudioSource.loop = isLoop;
                BGAudioAudioSource.volume = BGAudioVolumeValue;
                BGAudioAudioSource.Play();
            }
        }

        public void StopBGMusic()
        {
            if (BGAudioAudioSource != null)
            {
                BGAudioAudioSource.Stop();
            }
        }

        public async void PlayUIAudio(string name, bool isCache = true)
        {
            string path = bgAudioPath + name;
            AudioClip audioClip = await ResSvc.MainInstance.LoadAudioClipAsync(path, isCache);
            UIAudioAudioSource.PlayOneShot(audioClip, UIAudioVolumeValue);
        }

        public void PlayFootStep()
        {
            int i = Random.Range(0, CharacterFootSteps.Length);
            CharacterAudioSource.PlayOneShot(CharacterFootSteps[i], CharacterAudioVolumeValue);
        }

        public void PlayJumpEffort()
        {
            int i = Random.Range(0, CharacterJumpEfforts.Length);
            CharacterAudioSource.PlayOneShot(CharacterJumpEfforts[i], CharacterAudioVolumeValue);
        }

        public void PlayLanding()
        {
            int i = Random.Range(0, CharacterLanding.Length);
            CharacterAudioSource.PlayOneShot(CharacterLanding[i], CharacterAudioVolumeValue);
        }

        public void PlayHit()
        {
            int i = Random.Range(0, CharacterHit.Length);
            CharacterAudioSource.PlayOneShot(CharacterHit[i], CharacterAudioVolumeValue);
        }
        #endregion
    }
}
