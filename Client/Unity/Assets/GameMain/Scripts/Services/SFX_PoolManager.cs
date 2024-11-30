using HuHu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class SFX_PoolManager : Singleton<SFX_PoolManager>
    {
        //有SoundName的代表分的更细的音频
        [System.Serializable]
        public class SoundItem
        {
            public SoundStyle soundStyle;
            public string soundName;
            public string soundPath;
            public int soundCount;
            public bool AllowRepeat;
        }

        [SerializeField] private List<SoundItem> soundPools = new List<SoundItem>();
        private Dictionary<SoundStyle, Queue<GameObject>> soundCenter = new Dictionary<SoundStyle, Queue<GameObject>>();

        protected override void Awake()
        {
            base.Awake();
        }

        public async void InitSoundPool()
        {
            if (soundPools.Count == 0) { return; }
            for (int i = 0; i < soundPools.Count; i++)
            {
                for (int j = 0; j < soundPools[i].soundCount; j++)
                {
                    //实例化
                    var go = await ResSvc.MainInstance.LoadGameObjectAsync(Constants.ResourcePackgeName, soundPools[i].soundPath, Vector3.zero, Quaternion.Euler(Vector3.zero), Vector3.one, true, false, false, this.transform, default, false);
                    //设置父级点
                    go.transform.parent = this.transform;
                    //掩藏
                    go.SetActive(false);
                    //放入字典
                    if (!soundCenter.ContainsKey(soundPools[i].soundStyle))
                    {
                        //加入Kay
                        soundCenter.Add(soundPools[i].soundStyle, new Queue<GameObject>());
                        //把新实例加入Value，而不是预制体
                        soundCenter[soundPools[i].soundStyle].Enqueue(go);
                    }
                    else
                    {
                        //只用加Value
                        if (soundPools[i].AllowRepeat)
                        {
                            soundCenter[soundPools[i].soundStyle].Enqueue(go);
                        }
                        else
                        {
                            PECommon.Log("You try to add same SoundStyle item,but not allowed repeat SoundItem.The remaining item will be ignored.", PELogType.Warn);
                        }
                    }
                }
            }
        }

        public void TryPlaySoundFromPool(SoundStyle soundStye, Vector3 position, Quaternion quaternion, bool isPlayOnAwake = false, bool isLoop = false, float volume = 1f)
        {
            if (soundCenter.TryGetValue(soundStye, out var sound))
            {
                // Debug.Log(soundStye + "播放");
                GameObject go = sound.Dequeue();
                go.transform.position = position;
                go.transform.rotation = quaternion;
                go.SetActive(true);

                StartCoroutine(PlaySound(go.GetComponent<AudioSource>(), isPlayOnAwake, isLoop, volume));

                soundCenter[soundStye].Enqueue(go);
            }
            else
            {
                // Debug.Log(soundStye + "不存在");
            }
        }

        IEnumerator PlaySound(AudioSource audioSource, bool isPlayOnAwake, bool isLoop, float volume)
        {
            if (!isPlayOnAwake)
            {
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.loop = isLoop;
                    audioSource.volume = volume;
                    audioSource.Play();
                }
            }

            yield return new WaitForSeconds(audioSource.clip.length);
            audioSource.gameObject.SetActive(false);
        }

        public void TryStopSoundFromPool(SoundStyle soundStye)
        {
            if (soundCenter.TryGetValue(soundStye, out var sound))
            {
                GameObject go = sound.Dequeue();
                go.GetComponent<AudioSource>().Stop();

                soundCenter[soundStye].Enqueue(go);
            }
        }

        public void TryPauseSoundFromPool(SoundStyle soundStye)
        {
            if (soundCenter.TryGetValue(soundStye, out var sound))
            {
                GameObject go = sound.Dequeue();
                go.GetComponent<AudioSource>().Pause();

                soundCenter[soundStye].Enqueue(go);
            }
        }

        public void TryUnPauseSoundFromPool(SoundStyle soundStye)
        {
            if (soundCenter.TryGetValue(soundStye, out var sound))
            {
                GameObject go = sound.Dequeue();
                go.GetComponent<AudioSource>().UnPause();

                soundCenter[soundStye].Enqueue(go);
            }
        }

        public enum SoundStyle
        {
            Null,
            StateFootSteps_01,
            StateFootSteps_02,
            StateFootSteps_03,
            StateJumpEfforts,
            StateJumpLanding,
            StateHit,
            StateSkill_01,
            StateSkill_02,
            StateSkill_03,
            StateSkill_04,
            StateSkill_05,
            StateDie_01,
        }

    }
}
