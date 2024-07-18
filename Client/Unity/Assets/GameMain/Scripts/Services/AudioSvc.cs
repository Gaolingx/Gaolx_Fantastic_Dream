//功能：音频播放服务
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;
using static DarkGod.Main.SFX_PoolManager;

namespace DarkGod.Main
{
    public class AudioSvc : Singleton<AudioSvc>
    {
        [Range(0, 1)] public float BGAudioVolumeValue, UIAudioVolumeValue, CharacterAudioVolumeValue, CharacterFxAudioVolumeValue;
        public AudioSource BGAudioAudioSource, UIAudioAudioSource;

        [System.Serializable]
        public class CharSoundItem
        {
            public SoundStyle soundStyle;
            public string soundName;
        }

        [SerializeField] private List<CharSoundItem> CharacterFootStepsLst = new List<CharSoundItem>();
        [SerializeField] private List<CharSoundItem> CharacterJumpEffortsLst = new List<CharSoundItem>();
        [SerializeField] private List<CharSoundItem> CharacterLandingLst = new List<CharSoundItem>();
        [SerializeField] private List<CharSoundItem> CharacterHitLst = new List<CharSoundItem>();

        private string bgAudioPath = PathDefine.bgAudioPath;

        private UIController uiController;
        private SFX_PoolManager sfxPoolManager;
        private BindableProperty<float> _characterAudioVolumeValue = new BindableProperty<float>();
        private bool _isTurnOnAudio = true;

        protected override void Awake()
        {
            base.Awake();
        }

        public void InitSvc()
        {
            AddBindablePropertyData();
            uiController = GameRoot.MainInstance.GetUIController();

            sfxPoolManager = SFX_PoolManager.MainInstance;
            sfxPoolManager.InitSoundPool();

            PECommon.Log("Init AudioSvc...");
        }

        private void Update()
        {
            RefreshAudioSourceVolume();
        }

        public void AddBindablePropertyData()
        {
            _characterAudioVolumeValue.OnValueChanged += OnUpdateVolumeState;
        }

        public void RmvBindablePropertyData()
        {
            _characterAudioVolumeValue.OnValueChanged -= OnUpdateVolumeState;
        }

        private void OnUpdateVolumeState(float value)
        {
            sfxPoolManager.TrySetAllSoundVolume(value);
        }

        public void SetAllAudioObjectMuted(bool state)
        {
            _isTurnOnAudio = !state;
        }

        public bool GetAllAudioObjectMuted()
        {
            return !_isTurnOnAudio;
        }

        private void RefreshAudioSourceVolume()
        {
            _characterAudioVolumeValue.Value = CharacterAudioVolumeValue;

            if (BGAudioAudioSource != null)
            {
                BGAudioAudioSource.volume = BGAudioVolumeValue;
            }
            if (UIAudioAudioSource != null)
            {
                UIAudioAudioSource.volume = UIAudioVolumeValue;
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
                _characterAudioVolumeValue.Value = 0f;
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

        public void PlayFootStep(Transform transform)
        {
            int i = Random.Range(0, CharacterFootStepsLst.Count);
            sfxPoolManager.TryGetSoundPool(CharacterFootStepsLst[i].soundStyle, CharacterFootStepsLst[i].soundName, transform.position, transform.rotation, _characterAudioVolumeValue.Value);
        }

        public void PlayJumpEffort(Transform transform)
        {
            int i = Random.Range(0, CharacterJumpEffortsLst.Count);
            sfxPoolManager.TryGetSoundPool(CharacterJumpEffortsLst[i].soundStyle, CharacterJumpEffortsLst[i].soundName, transform.position, transform.rotation, _characterAudioVolumeValue.Value);
        }

        public void PlayLanding(Transform transform)
        {
            int i = Random.Range(0, CharacterLandingLst.Count);
            sfxPoolManager.TryGetSoundPool(CharacterLandingLst[i].soundStyle, CharacterLandingLst[i].soundName, transform.position, transform.rotation, _characterAudioVolumeValue.Value);
        }

        public void PlayHit(Transform transform)
        {
            int i = Random.Range(0, CharacterHitLst.Count);
            sfxPoolManager.TryGetSoundPool(CharacterHitLst[i].soundStyle, CharacterHitLst[i].soundName, transform.position, transform.rotation, _characterAudioVolumeValue.Value);
        }
        #endregion
    }
}
