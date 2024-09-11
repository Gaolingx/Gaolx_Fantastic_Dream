using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using HuHu;

namespace DarkGod.Main
{
    public class SFX_PoolManager : Singleton<SFX_PoolManager>
    {
        //��SoundName�Ĵ���ֵĸ�ϸ����Ƶ
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
                        //ʵ����
                        var go = await ResSvc.MainInstance.LoadGameObjectAsync(Constants.ResourcePackgeName, soundPools[i].soundPath, Vector3.zero, Vector3.zero, Vector3.one, true, true, true, this.transform, false, false);
                        //���ø�����
                        go.transform.parent = this.transform;
                        //�ڲ�
                        go.SetActive(false);
                        if (!bigSoundCenter.ContainsKey(soundPools[i].soundName))
                        {
                            // PECommon.Log(soundPools[i].soundName + "��������");
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
                        //ʵ����
                        var go = await ResSvc.MainInstance.LoadGameObjectAsync(Constants.ResourcePackgeName, soundPools[i].soundPath, Vector3.zero, Vector3.zero, Vector3.one, true, true, true, this.transform, false, false);
                        //���ø�����
                        go.transform.parent = this.transform;
                        //�ڲ�
                        go.SetActive(false);
                        //�����ֵ�
                        if (!soundCenter.ContainsKey(soundPools[i].soundStyle))
                        {
                            //����Kay
                            soundCenter.Add(soundPools[i].soundStyle, new Queue<GameObject>());
                            //����ʵ������Value��������Ԥ����
                            soundCenter[soundPools[i].soundStyle].Enqueue(go);
                        }
                        else
                        {
                            //ֻ�ü�Value
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
                    // Debug.Log("��������" + soundName + "������" + soundStyle);

                }
                else
                {
                    // Debug.LogWarning(soundStyle + "�Ҳ���");
                }
            }
            else
            {
                // Debug.LogWarning(soundName + "�Ҳ���");
            }

        }

        public void TryPlaySoundFromPool(SoundStyle soundStye, Vector3 position, Quaternion quaternion)
        {
            if (soundCenter.TryGetValue(soundStye, out var sound))
            {
                // Debug.Log(soundStye + "����");
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
                // Debug.Log(soundStye + "������");
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
