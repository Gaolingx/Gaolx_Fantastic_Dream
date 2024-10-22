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
            public bool isRepeat;
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
                    var go = await ResSvc.MainInstance.LoadGameObjectAsync(Constants.ResourcePackgeName, soundPools[i].soundPath, Vector3.zero, Vector3.zero, Vector3.one, true, true, true, this.transform, null, false);
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
                        if (soundPools[i].isRepeat)
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

        public void TryPlaySoundFromPool(SoundStyle soundStye, Vector3 position, Quaternion quaternion, float vol = 1f)
        {
            if (soundCenter.TryGetValue(soundStye, out var sound))
            {
                // Debug.Log(soundStye + "播放");
                GameObject go = sound.Dequeue();
                go.transform.position = position;
                go.transform.rotation = quaternion;
                go.SetActive(true);

                AudioSource audioSource = go.GetComponent<AudioSource>();
                if (audioSource != null && audioSource.clip != null)
                {
                    vol = Mathf.Clamp01(vol);
                    audioSource.volume = vol;
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
