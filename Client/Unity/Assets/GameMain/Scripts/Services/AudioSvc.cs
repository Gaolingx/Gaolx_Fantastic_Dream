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
        public List<string> FootStepsAudioPaths, JumpEffortsAudioPaths, LandingAudioPaths, HitAudioPaths;

        private List<AudioClip> CharacterFootStepsLst = new List<AudioClip>();
        private List<AudioClip> CharacterJumpEffortsLst = new List<AudioClip>();
        private List<AudioClip> CharacterLandingLst = new List<AudioClip>();
        private List<AudioClip> CharacterHitLst = new List<AudioClip>();

        private string bgAudioPath = PathDefine.bgAudioPath;

        private UIController uiController;
        private bool _isTurnOnAudio = true;

        protected override void Awake()
        {
            base.Awake();
        }

        public void InitSvc()
        {
            InitCharacterAudioClipLst(FootStepsAudioPaths, JumpEffortsAudioPaths, LandingAudioPaths, HitAudioPaths);
            uiController = GameRoot.MainInstance.GetUIController();
            PECommon.Log("Init AudioSvc...");
        }

        private void Update()
        {
            RefreshAudioSourceVolume();
        }

        public void SetCharacterAudioSource(AudioSource audioSource)
        {
            CharacterAudioSource = audioSource;
        }

        public void SetAllAudioObjectMuted(bool state)
        {
            _isTurnOnAudio = !state;
        }

        public bool GetAllAudioObjectMuted()
        {
            return !_isTurnOnAudio;
        }

        public void InitCharacterAudioClipLst(List<string> name1, List<string> name2, List<string> name3, List<string> name4)
        {
            for (int i = 0; i < name1.Count; i++)
            {
                string path = bgAudioPath + name1[i];
                AudioClip audioClip = ResSvc.MainInstance.LoadAudioClipSync(Constants.ResourcePackgeName, path);
                CharacterFootStepsLst.Add(audioClip);
            }

            for (int i = 0; i < name2.Count; i++)
            {
                string path = bgAudioPath + name2[i];
                AudioClip audioClip = ResSvc.MainInstance.LoadAudioClipSync(Constants.ResourcePackgeName, path);
                CharacterJumpEffortsLst.Add(audioClip);
            }

            for (int i = 0; i < name3.Count; i++)
            {
                string path = bgAudioPath + name3[i];
                AudioClip audioClip = ResSvc.MainInstance.LoadAudioClipSync(Constants.ResourcePackgeName, path);
                CharacterLandingLst.Add(audioClip);
            }

            for (int i = 0; i < name4.Count; i++)
            {
                string path = bgAudioPath + name4[i];
                AudioClip audioClip = ResSvc.MainInstance.LoadAudioClipSync(Constants.ResourcePackgeName, path);
                CharacterHitLst.Add(audioClip);
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

            if (!_isTurnOnAudio)
            {
                if (BGAudioAudioSource != null)
                {
                    BGAudioAudioSource.volume = 0f;
                }
                if (UIAudioAudioSource != null)
                {
                    UIAudioAudioSource.volume = 0f;
                }
                if (CharacterAudioSource != null)
                {
                    CharacterAudioSource.volume = 0f;
                }
            }
        }

        #region PlayAudio
        public async void PlayBGMusic(string name, bool isLoop = true, bool isCache = true)
        {
            string path = bgAudioPath + name;
            AudioClip audioClip = await ResSvc.MainInstance.LoadAudioClipAsync(Constants.ResourcePackgeName, path, isCache);
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
            AudioClip audioClip = await ResSvc.MainInstance.LoadAudioClipAsync(Constants.ResourcePackgeName, path, isCache);
            UIAudioAudioSource.PlayOneShot(audioClip, UIAudioVolumeValue);
        }

        public void PlayFootStep()
        {
            int i = Random.Range(0, CharacterFootStepsLst.Count);
            CharacterAudioSource.PlayOneShot(CharacterFootStepsLst[i], CharacterAudioVolumeValue);
        }

        public void PlayJumpEffort()
        {
            int i = Random.Range(0, CharacterJumpEffortsLst.Count);
            CharacterAudioSource.PlayOneShot(CharacterJumpEffortsLst[i], CharacterAudioVolumeValue);
        }

        public void PlayLanding()
        {
            int i = Random.Range(0, CharacterLandingLst.Count);
            CharacterAudioSource.PlayOneShot(CharacterLandingLst[i], CharacterAudioVolumeValue);
        }

        public void PlayHit()
        {
            int i = Random.Range(0, CharacterHitLst.Count);
            CharacterAudioSource.PlayOneShot(CharacterHitLst[i], CharacterAudioVolumeValue);
        }
        #endregion
    }
}
