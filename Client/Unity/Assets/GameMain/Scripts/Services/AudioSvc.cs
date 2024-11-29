//功能：音频播放服务

using Cysharp.Threading.Tasks;
using DarkGod.Tools;
using HuHu;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using static DarkGod.Main.SFX_PoolManager;

namespace DarkGod.Main
{
    public partial class CtsInfoList
    {
        public static CtsInfo stopPlayBGMCts;
    }

    public class AudioSvc : Singleton<AudioSvc>
    {
        public AudioSource BGAudioAudioSource;
        public AudioSource UIAudioAudioSource;
        public AudioMixer _audioMixer;

        [SerializeField] private float fadingDuration = 3f;

        private readonly string bgAudioPath = PathDefine.bgAudioPath;
        private Dictionary<string, CtsInfo> _ctsInfoDic = new Dictionary<string, CtsInfo>();

        private SFX_PoolManager sfxPoolManager;

        protected override void Awake()
        {
            base.Awake();

            EventMgr.MainInstance.OnGameEnter += delegate { InitSvc(); };
        }

        public void InitSvc()
        {
            sfxPoolManager = SFX_PoolManager.MainInstance;
            sfxPoolManager.InitSoundPool();

            AssignBindableData();

            PECommon.Log("Init AudioSvc...");
        }

        private void AssignBindableData()
        {
            QualitySvc.MainInstance.volume.BGAudioVolumeValue.OnValueChanged += delegate (float value) { BGAudioVolumeValueChanged(_audioMixer, value); };
            QualitySvc.MainInstance.volume.UIAudioVolumeValue.OnValueChanged += delegate (float value) { UIAudioVolumeValueChanged(_audioMixer, value); };
            QualitySvc.MainInstance.volume.CharacterAudioVolumeValue.OnValueChanged += delegate (float value) { CharacterAudioVolumeValueChanged(_audioMixer, value); };
            QualitySvc.MainInstance.volume.CharacterFxAudioVolumeValue.OnValueChanged += delegate (float value) { CharacterFxAudioVolumeValueChanged(_audioMixer, value); };

            QualitySvc.MainInstance.volume.BGAudioVolumeValue.Invoke();
            QualitySvc.MainInstance.volume.UIAudioVolumeValue.Invoke();
            QualitySvc.MainInstance.volume.CharacterAudioVolumeValue.Invoke();
            QualitySvc.MainInstance.volume.CharacterFxAudioVolumeValue.Invoke();
        }

        private void UnAssignBindableData()
        {
            QualitySvc.MainInstance.volume.BGAudioVolumeValue.OnValueChanged -= delegate (float value) { BGAudioVolumeValueChanged(_audioMixer, value); };
            QualitySvc.MainInstance.volume.UIAudioVolumeValue.OnValueChanged -= delegate (float value) { UIAudioVolumeValueChanged(_audioMixer, value); };
            QualitySvc.MainInstance.volume.CharacterAudioVolumeValue.OnValueChanged -= delegate (float value) { CharacterAudioVolumeValueChanged(_audioMixer, value); };
            QualitySvc.MainInstance.volume.CharacterFxAudioVolumeValue.OnValueChanged -= delegate (float value) { CharacterFxAudioVolumeValueChanged(_audioMixer, value); };
        }

        public void SetMainAudioMuted(bool state)
        {
            StartCoroutine(StartAudioFade(_audioMixer, "MainAudioVolumeParam", fadingDuration, UIItemUtils.BoolToInt(!state)));
        }
        private void BGAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "BGAudioVolumeParam", fadingDuration, value));
            EventMgr.OnSoundVolumeChangedEvent.SendEventMessage();
        }
        private void UIAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "UIAudioVolumeParam", fadingDuration, value));
            EventMgr.OnSoundVolumeChangedEvent.SendEventMessage();
        }
        private void CharacterAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "CharAudioVolumeParam", fadingDuration, value));
            EventMgr.OnSoundVolumeChangedEvent.SendEventMessage();
        }
        private void CharacterFxAudioVolumeValueChanged(AudioMixer audioMixer, float value)
        {
            StartCoroutine(StartAudioFade(audioMixer, "CharVFXAudioVolumeParam", fadingDuration, value));
            EventMgr.OnSoundVolumeChangedEvent.SendEventMessage();
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
            UIAudioAudioSource.PlayOneShot(audioClip, QualitySvc.MainInstance.volume.UIAudioVolumeValue.Value);
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
            EventMgr.MainInstance.OnGameEnter -= delegate { InitSvc(); };
            UnAssignBindableData();
            StopBGMusic();
        }
    }
}
