//功能：音频播放服务
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;
using static DarkGod.Main.SFX_PoolManager;
using UnityEngine.Audio;

namespace DarkGod.Main
{
    public class AudioSvc : Singleton<AudioSvc>
    {
        [Range(0, 1)] public float BGAudioVolumeValue, UIAudioVolumeValue, CharacterAudioVolumeValue, CharacterFxAudioVolumeValue;
        public AudioSource BGAudioAudioSource, UIAudioAudioSource;
        public AudioMixer _audioMixer;

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

        protected override void Awake()
        {
            base.Awake();
        }

        public void InitSvc()
        {
            uiController = GameRoot.MainInstance.GetUIController();

            sfxPoolManager = SFX_PoolManager.MainInstance;
            sfxPoolManager.InitSoundPool();

            PECommon.Log("Init AudioSvc...");
        }

        private void Update()
        {
            RefreshAudioSourceVolume();
        }

        public void SetMainAudioMuted(bool state)
        {
            _audioMixer.SetFloat("MainAudioVolumeParam", UIItemUtils.SetAudioVolumeVal(UIItemUtils.BoolToInt(!state)));
        }

        public void SetAudioListener(AudioListener playerAudioListener, bool statePlayer, bool stateGameRoot = false)
        {
            Transform gameRoot = transform.Find(Constants.Path_GameRoot_Obj);
            if (gameRoot != null)
            {
                gameRoot.gameObject.GetComponent<AudioListener>().enabled = stateGameRoot;
            }
            if (playerAudioListener != null)
            {
                playerAudioListener.enabled = statePlayer;
            }
        }

        private void RefreshAudioSourceVolume()
        {
            _audioMixer.SetFloat("BGAudioVolumeParam", UIItemUtils.SetAudioVolumeVal(BGAudioVolumeValue));
            _audioMixer.SetFloat("UIAudioVolumeParam", UIItemUtils.SetAudioVolumeVal(UIAudioVolumeValue));
            _audioMixer.SetFloat("CharAudioVolumeParam", UIItemUtils.SetAudioVolumeVal(CharacterAudioVolumeValue));
            _audioMixer.SetFloat("CharVFXAudioVolumeParam", UIItemUtils.SetAudioVolumeVal(CharacterFxAudioVolumeValue));
        }

        #region PlayAudio

        private Coroutine currentAudioCoroutine;
        public async void PlayBGMusics(List<string> names, float duration, bool isLoop = true, bool isCache = true)
        {
            List<AudioClip> audioClips = new List<AudioClip>();

            for (int i = 0; i < names.Count; i++)
            {
                audioClips.Add(await ResSvc.MainInstance.LoadAudioClipAsync(Constants.ResourcePackgeName, bgAudioPath + names[i], isCache));
            }
            currentAudioCoroutine = StartCoroutine(PlayAudioClips(audioClips, duration, isLoop));
        }

        IEnumerator PlayAudioClips(List<AudioClip> audioClips, float duration, bool isLoop)
        {
            while (isLoop)
            {
                if (audioClips == null || audioClips.Count == 0)
                    yield break; // 如果没有音频片段，则退出协程  

                for (int i = 0; i < audioClips.Count; i++)
                {
                    if (audioClips[i] != null)
                    {
                        BGAudioAudioSource.clip = audioClips[i];
                        BGAudioAudioSource.loop = false;
                        BGAudioAudioSource.volume = BGAudioVolumeValue;
                        BGAudioAudioSource.Play();
                        float targetValue = Mathf.Clamp(duration, 0f, 10f);
                        yield return new WaitForSeconds(audioClips[i].length + targetValue); // 等待当前音频播放完成  
                    }
                }
            }
        }

        public void StopBGMusic()
        {
            if (BGAudioAudioSource != null)
            {
                BGAudioAudioSource.Stop();
                if (currentAudioCoroutine != null)
                {
                    StopCoroutine(currentAudioCoroutine);
                    currentAudioCoroutine = null; // 可选：将引用设置为null，表示协程已停止  
                }
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
            sfxPoolManager.TryGetSoundPool(CharacterFootStepsLst[i].soundStyle, CharacterFootStepsLst[i].soundName, transform.position, transform.rotation);
        }

        public void PlayJumpEffort(Transform transform)
        {
            int i = Random.Range(0, CharacterJumpEffortsLst.Count);
            sfxPoolManager.TryGetSoundPool(CharacterJumpEffortsLst[i].soundStyle, CharacterJumpEffortsLst[i].soundName, transform.position, transform.rotation);
        }

        public void PlayLanding(Transform transform)
        {
            int i = Random.Range(0, CharacterLandingLst.Count);
            sfxPoolManager.TryGetSoundPool(CharacterLandingLst[i].soundStyle, CharacterLandingLst[i].soundName, transform.position, transform.rotation);
        }

        public void PlayHit(Transform transform)
        {
            int i = Random.Range(0, CharacterHitLst.Count);
            sfxPoolManager.TryGetSoundPool(CharacterHitLst[i].soundStyle, CharacterHitLst[i].soundName, transform.position, transform.rotation);
        }
        #endregion
    }
}
