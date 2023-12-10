using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//功能：飞龙循环动画
public class LoopDragonAni : MonoBehaviour
{
    private Animation ani;
    public float DragonAniRepeatTime = 0;
    public float DragonAniRepeatRate = 20;


    private void Awake()
    {
        ani=transform.GetComponent<Animation>();
    }

    private void Start()
    {
        if(ani != null)
        {
            InvokeRepeating("PlayDragonAni", DragonAniRepeatTime, DragonAniRepeatRate);
        }
    }

    private void PlayDragonAni()
    {
        if(ani != null)
        {
            ani.Play();
        }
        
    }
}
