using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerSoundController : MonoBehaviour
    {
        AudioSource source;
        public AudioClip[] footSteps;
        public AudioClip[] jumpEfforts;
        public AudioClip[] landing;
        [Range(0, 1)] public float footStepsAudioVolume = 0.5f;
        [Range(0, 1)] public float jumpEffortsAudioVolume = 0.5f;
        [Range(0, 1)] public float landingAudioVolume = 0.5f;
        // Start is called before the first frame update
        void Start()
        {
            source = GetComponent<AudioSource>();
        }

        public void PlayFootStep()
        {
            int i = Random.Range(0, footSteps.Length);
            source.PlayOneShot(footSteps[i], footStepsAudioVolume);
        }

        public void PlayJumpEffort()
        {
            int i = Random.Range(0, jumpEfforts.Length);
            source.PlayOneShot(jumpEfforts[i], jumpEffortsAudioVolume);
        }

        public void PlayLanding()
        {
            int i = Random.Range(0, landing.Length);
            source.PlayOneShot(landing[i], landingAudioVolume);
        }

    }
}//作者：IGBeginner0116 https://www.bilibili.com/read/cv18121672?spm_id_from=333.999.list.card_opus.click 出处：bilibili