//功能：音频播放服务
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class AudioSvc : MonoBehaviour
    {
        public static AudioSvc Instance = null;

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

        public void InitSvc()
        {
            Instance = this;

            InitAudioClipArray(FootStepsAudioPaths, JumpEffortsAudioPaths, LandingAudioPaths, HitAudioPaths);
            uiController = GameRoot.Instance.GetUIController();
            PECommon.Log("Init AudioSvc...");
        }

        private void Update()
        {
            RefreshAudioSourceVolume();
            if (uiController != null)
            {
                _isTurnOnAudio = !uiController._isPause;
            }
        }

        public void GetCharacterAudioSourceComponent(GameObject playerGO)
        {
            CharacterAudioSource = playerGO.GetComponent<AudioSource>();
            CharacterAudioSource.volume = CharacterAudioVolumeValue;
        }

        public void InitAudioClipArray(string[] name1, string[] name2, string[] name3, string[] name4)
        {
            for (int i = 0; i < name1.Length; i++)
            {
                string path = bgAudioPath + name1[i];
                AudioClip audioClip = ResSvc.Instance.LoadAudioClipSync(path);
                CharacterFootSteps[i] = audioClip;
            }

            for (int i = 0; i < name2.Length; i++)
            {
                string path = bgAudioPath + name2[i];
                AudioClip audioClip = ResSvc.Instance.LoadAudioClipSync(path);
                CharacterJumpEfforts[i] = audioClip;
            }

            for (int i = 0; i < name3.Length; i++)
            {
                string path = bgAudioPath + name3[i];
                AudioClip audioClip = ResSvc.Instance.LoadAudioClipSync(path);
                CharacterLanding[i] = audioClip;
            }

            for (int i = 0; i < name4.Length; i++)
            {
                string path = bgAudioPath + name4[i];
                AudioClip audioClip = ResSvc.Instance.LoadAudioClipSync(path);
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

        public void StopBGMusic()
        {
            if (BGAudioAudioSource != null)
            {
                BGAudioAudioSource.Stop();
            }
        }

        public async void PlayUIAudio(string name, bool isCache = true)
        {
            if (!_isTurnOnAudio) { return; }
            string path = bgAudioPath + name;
            AudioClip audioClip = await ResSvc.Instance.LoadAudioClipAsync(path, isCache);
            UIAudioAudioSource.PlayOneShot(audioClip, UIAudioVolumeValue);
        }

        public void PlayFootStep()
        {
            if (!_isTurnOnAudio) { return; }
            int i = Random.Range(0, CharacterFootSteps.Length);
            CharacterAudioSource.PlayOneShot(CharacterFootSteps[i], CharacterAudioVolumeValue);
        }

        public void PlayJumpEffort()
        {
            if (!_isTurnOnAudio) { return; }
            int i = Random.Range(0, CharacterJumpEfforts.Length);
            CharacterAudioSource.PlayOneShot(CharacterJumpEfforts[i], CharacterAudioVolumeValue);
        }

        public void PlayLanding()
        {
            if (!_isTurnOnAudio) { return; }
            int i = Random.Range(0, CharacterLanding.Length);
            CharacterAudioSource.PlayOneShot(CharacterLanding[i], CharacterAudioVolumeValue);
        }

        public void PlayHit()
        {
            if (!_isTurnOnAudio) { return; }
            int i = Random.Range(0, CharacterHit.Length);
            CharacterAudioSource.PlayOneShot(CharacterHit[i], CharacterAudioVolumeValue);
        }
        #endregion
    }
}
