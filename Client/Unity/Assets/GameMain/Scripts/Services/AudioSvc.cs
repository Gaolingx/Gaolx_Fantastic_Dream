//功能：音频播放服务

using Cysharp.Threading.Tasks;
using DarkGod.Tools;
using HuHu;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using static DarkGod.Main.QualitySvc;
using static DarkGod.Main.SFX_PoolManager;

namespace DarkGod.Main
{
    public class AudioSvc : Singleton<AudioSvc>
    {
        public AudioSource BGAudioAudioSource;
        public AudioSource UIAudioAudioSource;
        public AudioMixer _audioMixer;

        public float fadingDuration = 3f;

        public SoundVolume volume { get; private set; } = new SoundVolume();

        private readonly string bgAudioPath = PathDefine.bgAudioPath;
        private Dictionary<string, CtsInfo> _ctsInfoDic = new Dictionary<string, CtsInfo>();

        private SFX_PoolManager sfxPoolManager;
        private const string prefsKey_SettingsAudioSvc = "prefsKey_SettingsAudioSvc";

        public class CtsInfoList
        {
            public static CtsInfo stopPlayBGMCts;
        }

        public class SoundVolume
        {
            public BindableProperty<float> BGAudioVolumeValue { get; set; } = new BindableProperty<float>(value: -0.01f);
            public BindableProperty<float> UIAudioVolumeValue { get; set; } = new BindableProperty<float>(value: -0.01f);
            public BindableProperty<float> CharacterAudioVolumeValue { get; set; } = new BindableProperty<float>(value: -0.01f);
            public BindableProperty<float> CharacterFxAudioVolumeValue { get; set; } = new BindableProperty<float>(value: -0.01f);
        }

        protected override void Awake()
        {
            base.Awake();

            GameStateEvent.MainInstance.OnGameEnter += delegate { InitSvc(); };
        }

        public void InitSvc()
        {
            sfxPoolManager = SFX_PoolManager.MainInstance;
            sfxPoolManager.InitSoundPool();

            AssignBindableData();
            InitVolumeData();

            PECommon.Log("Init AudioSvc...");
        }

        private void InitVolumeData()
        {
            if (PlayerPrefsSvc.MainInstance.CheckPlayerPrefsHasKey(prefsKey_SettingsAudioSvc))
            {
                var json = PlayerPrefsSvc.MainInstance.LoadFromPlayerPrefs(prefsKey_SettingsAudioSvc);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData2>(json);

                volume.BGAudioVolumeValue.Value = saveData.BGAudioVolume;
                volume.UIAudioVolumeValue.Value = saveData.UIAudioVolume;
                volume.CharacterAudioVolumeValue.Value = saveData.CharacterAudioVolume;
                volume.CharacterFxAudioVolumeValue.Value = saveData.CharacterFxAudioVolume;
            }
            else
            {
                volume.BGAudioVolumeValue.Value = 0.25f;
                volume.UIAudioVolumeValue.Value = 0.5f;
                volume.CharacterAudioVolumeValue.Value = 0.5f;
                volume.CharacterFxAudioVolumeValue.Value = 0.5f;
            }
        }

        private void AssignBindableData()
        {
            volume.BGAudioVolumeValue.OnValueChanged += delegate (float value) { BGAudioVolumeValueChanged(_audioMixer, value); };
            volume.UIAudioVolumeValue.OnValueChanged += delegate (float value) { UIAudioVolumeValueChanged(_audioMixer, value); };
            volume.CharacterAudioVolumeValue.OnValueChanged += delegate (float value) { CharacterAudioVolumeValueChanged(_audioMixer, value); };
            volume.CharacterFxAudioVolumeValue.OnValueChanged += delegate (float value) { CharacterFxAudioVolumeValueChanged(_audioMixer, value); };
        }

        private void UnAssignBindableData()
        {
            volume.BGAudioVolumeValue.OnValueChanged -= delegate (float value) { BGAudioVolumeValueChanged(_audioMixer, value); };
            volume.UIAudioVolumeValue.OnValueChanged -= delegate (float value) { UIAudioVolumeValueChanged(_audioMixer, value); };
            volume.CharacterAudioVolumeValue.OnValueChanged -= delegate (float value) { CharacterAudioVolumeValueChanged(_audioMixer, value); };
            volume.CharacterFxAudioVolumeValue.OnValueChanged -= delegate (float value) { CharacterFxAudioVolumeValueChanged(_audioMixer, value); };
        }

        private void SendMessageToEventMgr()
        {
            PlayerPrefsData2 data = new PlayerPrefsData2();
            data.BGAudioVolume = volume.BGAudioVolumeValue.Value;
            data.UIAudioVolume = volume.UIAudioVolumeValue.Value;
            data.CharacterAudioVolume = volume.CharacterAudioVolumeValue.Value;
            data.CharacterFxAudioVolume = volume.CharacterFxAudioVolumeValue.Value;

            EventMgr.OnSoundVolumeChangedEvent.SendEventMessage(data);
        }

        public void SetMainAudioMuted(bool state)
        {
            StartCoroutine(StartAudioFade(_audioMixer, "MainAudioVolumeParam", fadingDuration, UIItemUtils.BoolToInt(!state)));
        }
        private void BGAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "BGAudioVolumeParam", fadingDuration, value));
            SendMessageToEventMgr();
        }
        private void UIAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "UIAudioVolumeParam", fadingDuration, value));
            SendMessageToEventMgr();
        }
        private void CharacterAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "CharAudioVolumeParam", fadingDuration, value));
            SendMessageToEventMgr();
        }
        private void CharacterFxAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "CharVFXAudioVolumeParam", fadingDuration, value));
            SendMessageToEventMgr();
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

        public async void PlayBGMusics(List<string> names, float duration, bool isLoop = true, bool isCache = true, PlayerLoopTiming playerLoopTiming = PlayerLoopTiming.Update)
        {
            List<AudioClip> audioClips = new List<AudioClip>();

            for (int i = 0; i < names.Count; i++)
            {
                audioClips.Add(await ResSvc.MainInstance.LoadAudioClipAsync(Constants.ResourcePackgeName, $"{bgAudioPath}/{names[i]}", isCache));
            }

            CtsInfoList.stopPlayBGMCts = DelaySignalManager.MainInstance.CreatCts();
            CtsInfoList.stopPlayBGMCts.Token.Register(delegate { BGAudioAudioSource.Stop(); });

            await PlayAudioClips(audioClips, duration, isLoop, playerLoopTiming, CtsInfoList.stopPlayBGMCts.Token).SuppressCancellationThrow();
        }

        private async UniTask PlayAudioClips(List<AudioClip> audioClips, float duration, bool isLoop, PlayerLoopTiming playerLoopTiming, CancellationToken cts)
        {
            while (isLoop)
            {
                if (audioClips == null || audioClips.Count == 0)
                    break;

                for (int i = 0; i < audioClips.Count; i++)
                {
                    if (audioClips[i] != null)
                    {
                        BGAudioAudioSource.clip = audioClips[i];
                        BGAudioAudioSource.loop = false;
                        BGAudioAudioSource.Play();
                        float targetValue = Mathf.Clamp(duration, 0f, 10f);
                        await UniTask.Delay((int)(audioClips[i].length * 1000) + (int)(targetValue * 1000), false, playerLoopTiming, cts);
                    }
                }
            }
        }

        public void StopBGMusic()
        {
            DelaySignalManager.MainInstance.CancelTask(CtsInfoList.stopPlayBGMCts);
        }

        public async void PlayUIAudio(string name, bool isCache = true)
        {
            string path = $"{bgAudioPath}/{name}";
            AudioClip audioClip = await ResSvc.MainInstance.LoadAudioClipAsync(Constants.ResourcePackgeName, path, isCache);
            UIAudioAudioSource.PlayOneShot(audioClip, volume.UIAudioVolumeValue.Value);
        }

        public void PlayFootStep(Transform transform)
        {
            sfxPoolManager.TryPlaySoundFromPool(SoundStyle.StateFootSteps_01, transform.position, transform.rotation);
        }

        public void PlayJumpEffort(Transform transform)
        {
            sfxPoolManager.TryPlaySoundFromPool(SoundStyle.StateJumpEfforts, transform.position, transform.rotation);
        }

        public void PlayLanding(Transform transform)
        {
            sfxPoolManager.TryPlaySoundFromPool(SoundStyle.StateJumpLanding, transform.position, transform.rotation);
        }

        public void PlayHit(Transform transform)
        {
            sfxPoolManager.TryPlaySoundFromPool(SoundStyle.StateHit, transform.position, transform.rotation);
        }
        #endregion

        private void OnDisable()
        {
            GameStateEvent.MainInstance.OnGameEnter -= delegate { InitSvc(); };
            UnAssignBindableData();
            StopBGMusic();
        }
    }
}
