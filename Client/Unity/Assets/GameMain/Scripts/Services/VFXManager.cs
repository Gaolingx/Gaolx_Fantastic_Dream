using Cysharp.Threading.Tasks;
using HuHu;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class VFXManager : Singleton<VFXManager>
    {
        private readonly Dictionary<string, EffectPool> _EffectPoolCache = new Dictionary<string, EffectPool>();

        private TimerSvc timerSvc;

        protected override void Awake()
        {
            base.Awake();

            GameStateEvent.MainInstance.OnGameEnter += delegate { InitMgr(); };
        }

        public void InitMgr()
        {
            timerSvc = TimerSvc.MainInstance;
        }

        public async UniTask PreLoadEffect(string effectName)
        {
            if (!(effectName == "") && !_EffectPoolCache.ContainsKey(effectName))
            {
                GameObject go = await ResSvc.MainInstance.LoadGameObjectAsync(Constants.ResourcePackgeName, effectName, Vector3.zero, Quaternion.Euler(Vector3.zero), Vector3.one, null, true);
                if (!_EffectPoolCache.TryGetValue(effectName, out var value))
                {
                    value = new EffectPool(go);
                    _EffectPoolCache.Add(effectName, value);
                }
            }
        }

        private async UniTask<GameObject> GetEffectFromPool(string effectName)
        {
            if (effectName == "")
            {
                return null;
            }
            GameObject result;
            if (_EffectPoolCache.TryGetValue(effectName, out var value))
            {
                result = value.Get();
            }
            else
            {
                GameObject go = await ResSvc.MainInstance.LoadGameObjectAsync(Constants.ResourcePackgeName, effectName, Vector3.zero, Quaternion.Euler(Vector3.zero), Vector3.one, null, true);
                if (!_EffectPoolCache.TryGetValue(effectName, out var value2))
                {
                    value2 = new EffectPool(go);
                    _EffectPoolCache.Add(effectName, value2);
                }
                result = value2.Get();
            }
            return result;
        }

        public async UniTask<GameObject> Play(string effectName, Vector3 position, Quaternion rotation, float destroy, Transform parent = null, Action onComplete = null, float scale = 1f)
        {
            if (effectName == "")
            {
                return null;
            }
            GameObject gameEffect = await GetEffectFromPool(effectName);
            if (parent != null)
            {
                gameEffect.transform.SetParent(parent);
            }
            gameEffect.transform.position = position;
            gameEffect.transform.rotation = rotation;
            gameEffect.transform.localScale = Vector3.one * scale;
            PlayFromPool(gameEffect, destroy, onComplete);
            return gameEffect;
        }

        private void PlayFromPool(GameObject go, float destroy, Action action)
        {
            if (go != null)
            {
                AudioSource audio = null;
                if (go.TryGetComponent<AudioSource>(out var audioSource))
                {
                    audio = audioSource;
                }
                ParticleSystem[] particles = go.GetComponentsInChildren<ParticleSystem>();

                go.SetActive(true);
                if (audio != null)
                {
                    audio.Play();
                }
                foreach (ParticleSystem particle in particles)
                {
                    if (!particle.main.playOnAwake)
                        particle.Play();
                }

                timerSvc.AddTimeTask((int tid) =>
                {
                    if (audio != null)
                    {
                        audio.Stop();
                    }
                    foreach (ParticleSystem particle in particles)
                    {
                        if (!particle.main.playOnAwake)
                            particle.Stop();
                    }
                    go.SetActive(false);
                }, destroy);
            }

            action?.Invoke();
        }

        public void Stop(string effectName, GameObject _effect)
        {
            if (_effect != null && _EffectPoolCache.TryGetValue(effectName, out var value))
            {
                value.Release(_effect);
            }
        }

        private void OnDisable()
        {
            GameStateEvent.MainInstance.OnGameEnter -= delegate { InitMgr(); };
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<string, EffectPool> item in _EffectPoolCache)
            {
                item.Value.OnDestroy();
            }
            _EffectPoolCache.Clear();
        }
    }
}
