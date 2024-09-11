using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using HuHu;

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
            public bool ApplyBigCenter;
        }

        [SerializeField] private List<SoundItem> soundPools = new List<SoundItem>();
        private Dictionary<SoundStyle, Queue<GameObject>> soundCenter = new Dictionary<SoundStyle, Queue<GameObject>>();
        private Dictionary<string, Dictionary<SoundStyle, Queue<GameObject>>> bigSoundCenter = new Dictionary<string, Dictionary<SoundStyle, Queue<GameObject>>>();

        protected override void Awake()
        {
            base.Awake();
        }

        public async void InitSoundPool()
        {
            if (soundPools.Count == 0) { return; }
            for (int i = 0; i < soundPools.Count; i++)
            {
                if (soundPools[i].ApplyBigCenter)
                {
                    for (int j = 0; j < soundPools[i].soundCount; j++)
                    {
                        //实例化
                        var go = await ResSvc.MainInstance.LoadGameObjectAsync(Constants.ResourcePackgeName, soundPools[i].soundPath, Vector3.zero, Vector3.zero, Vector3.one, true, true, true, this.transform, false, false);
                        //设置父级点
                        go.transform.parent = this.transform;
                        //掩藏
                        go.SetActive(false);
                        if (!bigSoundCenter.ContainsKey(soundPools[i].soundName))
                        {
                            // PECommon.Log(soundPools[i].soundName + "加入对象池");
                            bigSoundCenter.Add(soundPools[i].soundName, new Dictionary<SoundStyle, Queue<GameObject>>());
                        }
                        if (!bigSoundCenter[soundPools[i].soundName].ContainsKey(soundPools[i].soundStyle))
                        {
                            bigSoundCenter[soundPools[i].soundName].Add(soundPools[i].soundStyle, new Queue<GameObject>());
                        }
                        bigSoundCenter[soundPools[i].soundName][soundPools[i].soundStyle].Enqueue(go);
                    }
                }
                else
                {
                    for (int j = 0; j < soundPools[i].soundCount; j++)
                    {
                        //实例化
                        var go = await ResSvc.MainInstance.LoadGameObjectAsync(Constants.ResourcePackgeName, soundPools[i].soundPath, Vector3.zero, Vector3.zero, Vector3.one, true, true, true, this.transform, false, false);
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
                            soundCenter[soundPools[i].soundStyle].Enqueue(go);
                        }
                    }
                }

            }

        }

        public void TryPlaySoundFromPool(SoundStyle soundStyle, string soundName, Vector3 position, Quaternion quaternion)
        {
            if (bigSoundCenter.ContainsKey(soundName))
            {
                if (bigSoundCenter[soundName].TryGetValue(soundStyle, out var Q))
                {
                    GameObject go = Q.Dequeue();
                    go.transform.position = position;
                    go.transform.rotation = quaternion;
                    go.gameObject.SetActive(true);

                    AudioSource audioSource = go.GetComponent<AudioSource>();
                    if (audioSource != null && audioSource.clip != null)
                    {
                        audioSource.Play();
                    }

                    Q.Enqueue(go);
                    // Debug.Log("播放音乐" + soundName + "类型是" + soundStyle);

                }
                else
                {
                    // Debug.LogWarning(soundStyle + "找不到");
                }
            }
            else
            {
                // Debug.LogWarning(soundName + "找不到");
            }

        }

        public void TryPlaySoundFromPool(SoundStyle soundStye, Vector3 position, Quaternion quaternion)
        {
            if (soundCenter.TryGetValue(soundStye, out var sound))
            {
                // Debug.Log(soundStye + "播放");
                GameObject go = sound.Dequeue();
                go.transform.position = position;
                go.transform.rotation = quaternion;
                go.gameObject.SetActive(true);

                AudioSource audioSource = go.GetComponent<AudioSource>();
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.Play();
                }

                soundCenter[soundStye].Enqueue(go);
            }
            else
            {
                // Debug.Log(soundStye + "不存在");
            }
        }

        public enum SoundStyle
        {
            Null,
            StateFootSteps_01,
            StateFootSteps_02,
            StateFootSteps_03,
            StateFootSteps_04,
            StateFootSteps_05,
            StateFootSteps_06,
            StateFootSteps_07,
            StateFootSteps_08,
            StateFootSteps_09,
            StateFootSteps_10,
            StateJumpEfforts,
            StateLanding,
            StateHit,

        }

    }
}
