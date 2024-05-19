/****************************************************
    文件：LoopDragonAni.cs
	作者：Plane
    邮箱: 1785275942@qq.com
    日期：2018/12/4 5:51:47
	功能：飞龙循环动画
*****************************************************/

using UnityEngine;

namespace DarkGod.Main
{
    public class LoopDragonAni : MonoBehaviour
    {
        public float AniRepeatTime = 20.0f;

        private Animation ani;

        private void Awake()
        {
            ani = transform.GetComponent<Animation>();
        }

        private void Start()
        {
            if (ani != null)
            {
                InvokeRepeating("PlayDragonAni", 0, AniRepeatTime);
            }
        }

        private void PlayDragonAni()
        {
            if (ani != null)
            {
                ani.Play();
            }
        }
    }
}