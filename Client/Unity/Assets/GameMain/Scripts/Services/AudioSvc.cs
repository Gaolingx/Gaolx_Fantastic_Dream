//���ܣ���Ƶ���ŷ���

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;
using static DarkGod.Main.SFX_PoolManager;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

namespace DarkGod.Main
{
    public class AudioSvc : Singleton<AudioSvc>
    {
        public BindableProperty<float> BGAudioVolumeValue { get; set; } = new BindableProperty<float>();
        public BindableProperty<float> UIAudioVolumeValue { get; set; } = new BindableProperty<float>();
        public BindableProperty<float> CharacterAudioVolumeValue { get; set; } = new BindableProperty<float>();
        public BindableProperty<float> CharacterFxAudioVolumeValue { get; set; } = new BindableProperty<float>();

        public AudioSource BGAudioAudioSource, UIAudioAudioSource;
        public AudioMixer _audioMixer;

        [SerializeField] private float fadingDuration = 3f;

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

        private readonly string bgAudioPath = PathDefine.bgAudioPath;

        private SFX_PoolManager sfxPoolManager;
        private CancellationTokenSourceMgr ctsMgr;
        private PlayerPrefsSvc playerPrefsSvc;

        private const string prefsKey_SettingsAudioSvc = "prefsKey_SettingsAudioSvc";

        [HideInInspector]
        [System.Serializable]
        private class PlayerPrefsData
        {
            public float BGAudioVolume;
            public float UIAudioVolume;
            public float CharacterAudioVolume;
            public float CharacterFxAudioVolume;
        }

        private void LoadPrefsData()
        {
            BGAudioVolumeValue.Value = 0.25f;
            UIAudioVolumeValue.Value = 0.5f;
            CharacterAudioVolumeValue.Value = 0.5f;
            CharacterFxAudioVolumeValue.Value = 0.5f;

            if (playerPrefsSvc.CheckPlayerPrefsHasKey(prefsKey_SettingsAudioSvc))
            {
                var json = playerPrefsSvc.LoadFromPlayerPrefs(prefsKey_SettingsAudioSvc);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData>(json);

                BGAudioVolumeValue.Value = saveData.BGAudioVolume;
                UIAudioVolumeValue.Value = saveData.UIAudioVolume;
                CharacterAudioVolumeValue.Value = saveData.CharacterAudioVolume;
                CharacterFxAudioVolumeValue.Value = saveData.CharacterFxAudioVolume;
            }
        }

        private void SavePrefsData()
        {
            var saveData = new PlayerPrefsData();

            saveData.BGAudioVolume = BGAudioVolumeValue.Value;
            saveData.UIAudioVolume = UIAudioVolumeValue.Value;
            saveData.CharacterAudioVolume = CharacterAudioVolumeValue.Value;
            saveData.CharacterFxAudioVolume = CharacterFxAudioVolumeValue.Value;

            playerPrefsSvc.SaveByPlayerPrefs(prefsKey_SettingsAudioSvc, saveData);
        }

        protected override void Awake()
        {
            base.Awake();

            GameRoot.MainInstance.OnGameEnter += InitSvc;
        }

        private void FixedUpdate()
        {
            if (playerPrefsSvc != null)
            {
                SavePrefsData();
            }
        }

        public void InitSvc()
        {
            sfxPoolManager = SFX_PoolManager.MainInstance;
            sfxPoolManager.InitSoundPool();

            ctsMgr = CancellationTokenSourceMgr.MainInstance;
            playerPrefsSvc = PlayerPrefsSvc.MainInstance;

            AssignBindableData();
            LoadPrefsData();

            PECommon.Log("Init AudioSvc...");
        }

        public void SetMainAudioMuted(bool state)
        {
            StartCoroutine(StartAudioFade(_audioMixer, "MainAudioVolumeParam", fadingDuration, UIItemUtils.BoolToInt(!state)));
        }

        private void AssignBindableData()
        {
            BGAudioVolumeValue.OnValueChanged += delegate (float value) { BGAudioVolumeValueChanged(_audioMixer, value); };
            UIAudioVolumeValue.OnValueChanged += delegate (float value) { UIAudioVolumeValueChanged(_audioMixer, value); };
            CharacterAudioVolumeValue.OnValueChanged += delegate (float value) { CharacterAudioVolumeValueChanged(_audioMixer, value); };
            CharacterFxAudioVolumeValue.OnValueChanged += delegate (float value) { CharacterFxAudioVolumeValueChanged(_audioMixer, value); };
        }

        private void BGAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "BGAudioVolumeParam", fadingDuration, value));
        }
        private void UIAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "UIAudioVolumeParam", fadingDuration, value));
        }
        private void CharacterAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "CharAudioVolumeParam", fadingDuration, value));
        }
        private void CharacterFxAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "CharVFXAudioVolumeParam", fadingDuration, value));
        }

        public static IEnumerator StartAudioFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
        {
            float currentTime = 0;
            float currentVol;
            audioMixer.GetFloat(exposedParam, out currentVol);
            currentVol = Mathf.Pow(10, currentVol / 20);
            float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
                audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
                yield return null;
            }
            yield break;
        }

        #region PlayAudio

        public async void PlayBGMusics(List<string> names, float duration, bool isLoop = true, bool isCache = true, CtsType ctsType = CtsType.PlayBGM)
        {
            List<AudioClip> audioClips = new List<AudioClip>();
            ctsMgr.SetCtsValue(ctsType, new CancellationTokenSource());

            for (int i = 0; i < names.Count; i++)
            {
                audioClips.Add(await ResSvc.MainInstance.LoadAudioClipAsync(Constants.ResourcePackgeName, bgAudioPath + names[i], isCache));
            }

            //����ȡ���첽�׳����쳣
            try
            {
                await PlayAudioClips(audioClips, duration, isLoop).ToUniTask(PlayerLoopTiming.Update, ctsMgr.GetCtsValue(ctsType).Token);
            }
            catch (System.OperationCanceledException ex)
            {
                PECommon.Log($"Operation cancelled:{ex?.ToString()}", PELogType.Warn);
            }

        }

        IEnumerator PlayAudioClips(List<AudioClip> audioClips, float duration, bool isLoop)
        {
            while (isLoop)
            {
                if (audioClips == null || audioClips.Count == 0)
                    yield break; // ���û����ƵƬ�Σ����˳�Э��  

                for (int i = 0; i < audioClips.Count; i++)
                {
                    if (audioClips[i] != null)
                    {
                        BGAudioAudioSource.clip = audioClips[i];
                        BGAudioAudioSource.loop = false;
                        BGAudioAudioSource.Play();
                        float targetValue = Mathf.Clamp(duration, 0f, 10f);
                        yield return new WaitForSeconds(audioClips[i].length + targetValue); // �ȴ���ǰ��Ƶ�������  
                    }
                }
            }
        }

        public void StopBGMusic(CtsType ctsType = CtsType.PlayBGM)
        {
            if (BGAudioAudioSource != null)
            {
                BGAudioAudioSource.Stop();
            }

            ctsMgr.GetCtsValue(ctsType)?.Cancel();
        }

        public async void PlayUIAudio(string name, bool isCache = true)
        {
            string path = bgAudioPath + name;
            AudioClip audioClip = await ResSvc.MainInstance.LoadAudioClipAsync(Constants.ResourcePackgeName, path, isCache);
            UIAudioAudioSource.PlayOneShot(audioClip, UIAudioVolumeValue.Value);
        }

        public void PlayFootStep(Transform transform)
        {
            int i = Random.Range(0, CharacterFootStepsLst.Count);
            sfxPoolManager.TryPlaySoundFromPool(CharacterFootStepsLst[i].soundStyle, CharacterFootStepsLst[i].soundName, transform.position, transform.rotation);
        }

        public void PlayJumpEffort(Transform transform)
        {
            int i = Random.Range(0, CharacterJumpEffortsLst.Count);
            sfxPoolManager.TryPlaySoundFromPool(CharacterJumpEffortsLst[i].soundStyle, CharacterJumpEffortsLst[i].soundName, transform.position, transform.rotation);
        }

        public void PlayLanding(Transform transform)
        {
            int i = Random.Range(0, CharacterLandingLst.Count);
            sfxPoolManager.TryPlaySoundFromPool(CharacterLandingLst[i].soundStyle, CharacterLandingLst[i].soundName, transform.position, transform.rotation);
        }

        public void PlayHit(Transform transform)
        {
            int i = Random.Range(0, CharacterHitLst.Count);
            sfxPoolManager.TryPlaySoundFromPool(CharacterHitLst[i].soundStyle, CharacterHitLst[i].soundName, transform.position, transform.rotation);
        }
        #endregion

        private void OnDestroy()
        {
            GameRoot.MainInstance.OnGameEnter -= InitSvc;
            BGAudioVolumeValue.OnValueChanged -= delegate (float value) { BGAudioVolumeValueChanged(_audioMixer, value); };
            UIAudioVolumeValue.OnValueChanged -= delegate (float value) { UIAudioVolumeValueChanged(_audioMixer, value); };
            CharacterAudioVolumeValue.OnValueChanged -= delegate (float value) { CharacterAudioVolumeValueChanged(_audioMixer, value); };
            CharacterFxAudioVolumeValue.OnValueChanged -= delegate (float value) { CharacterFxAudioVolumeValueChanged(_audioMixer, value); };
        }
    }
}
