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
        [Header("特效播放倍率")] public float SpeedMult;
        public List<ParticleSystem> allParticleSystems => particleSystems;

        private List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        private Dictionary<string, GameObject> fxObjDic = new Dictionary<string, GameObject>();

        private TimerSvc timerSvc;

        protected override void Awake()
        {
            base.Awake();

            GameRoot.MainInstance.OnGameEnter += InitFX;
        }

        private void AddVFXOnInit()
        {
            for (int i = 0; i < PlayerFxList.Count; i++)
            {
                fxObjDic.Add(PlayerFxList[i].name, PlayerFxList[i]);
            }

            for (int i = 0; i < PlayerFxList.Count; i++)
            {
                particleSystems.Add(PlayerFxList[i].GetComponent<ParticleSystem>());
            }
            foreach (var particle in particleSystems)
            {
                var main = particle.main;
                main.simulationSpeed = SpeedMult;
            }
        }

        public void InitFX()
        {
            timerSvc = TimerSvc.MainInstance;

            AddVFXOnInit();
        }

        public void SetFX(Transform parent, string name, float destroy)
        {
            GameObject go;
            if (fxObjDic.TryGetValue(name, out go))
            {
                go.transform.SetParent(parent, false);
                AudioSource audioSource = go.GetComponent<AudioSource>();
                ParticleSystem particle = go.GetComponent<ParticleSystem>();
                if (audioSource != null)
                {
                    audioSource.Play();
                }
                if (particle != null)
                {
                    particle.Play();
                }
                go.SetActive(true);
                timerSvc.AddTimeTask((int tid) =>
                {
                    if (audioSource != null)
                    {
                        audioSource.Stop();
                    }
                    if (particle != null)
                    {
                        particle.Stop();
                    }
                    go.SetActive(false);
                }, destroy);
            }
        }

        public void AddVFX(ParticleSystem particleSystem, float speedMult = 1f)
        {
            fxObjDic.Add(particleSystem.gameObject.name, particleSystem.gameObject);
            particleSystems.Add(particleSystem);
            foreach (var particle in particleSystems)
            {
                var main = particle.main;
                main.simulationSpeed = speedMult;
            }
        }

        public void PauseVFX()
        {
            foreach (var particleSystem in allParticleSystems)
            {
                var main = particleSystem.main;
                main.simulationSpeed = 0f;
            }

        }
        public void SetVFXSpeed(float speedMult)
        {
            foreach (var particleSystem in allParticleSystems)
            {
                var main = particleSystem.main;
                main.simulationSpeed = speedMult;
            }
        }
        public void ResetVFX()
        {
            foreach (var particleSystem in allParticleSystems)
            {
                var main = particleSystem.main;
                main.simulationSpeed = SpeedMult;
            }

        }

        private void OnDestroy()
        {
            GameRoot.MainInstance.OnGameEnter -= InitFX;
        }
    }
}
