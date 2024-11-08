using HuHu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class VFXManager : Singleton<VFXManager>
    {
        [Header("Player FX GameObject")]
        public List<GameObject> PlayerFxList = new List<GameObject>();

        private Dictionary<string, GameObject> particleSystems = new Dictionary<string, GameObject>();

        private TimerSvc timerSvc;

        protected override void Awake()
        {
            base.Awake();

            EventMgr.MainInstance.OnGameEnter += delegate { InitMgr(); };
        }

        private void AddVFXOnInit()
        {
            foreach (var item in PlayerFxList)
            {
                particleSystems.Add(item.name, item);
            }
        }

        public void InitMgr()
        {
            timerSvc = TimerSvc.MainInstance;

            AddVFXOnInit();
        }

        public void SetFX(Transform parent, string name, float destroy)
        {
            if (particleSystems.TryGetValue(name, out GameObject asset))
            {
                GameObject go = Instantiate(asset, parent);
                go.SetActive(true);
                ParticleSystem[] particles = go.GetComponentsInChildren<ParticleSystem>();
                if (go.TryGetComponent<AudioSource>(out var audioSource))
                {
                    audioSource.Play();
                }
                foreach (ParticleSystem particle in particles)
                {
                    particle.Play();
                }

                timerSvc.AddTimeTask((int tid) =>
                {
                    if (audioSource != null)
                    {
                        audioSource.Stop();
                    }
                    foreach (ParticleSystem particle in particles)
                    {
                        particle.Stop();
                    }
                    go.SetActive(false);
                }, destroy);
            }
        }

        public void AddVFX(List<GameObject> particleLst)
        {
            foreach (var item in particleLst)
            {
                particleSystems.Add(item.name, item);
            }
        }

        private void OnDisable()
        {
            EventMgr.MainInstance.OnGameEnter -= delegate { InitMgr(); };
        }
    }
}
