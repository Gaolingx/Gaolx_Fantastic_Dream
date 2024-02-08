//功能：动态UI元素界面
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DynamicWnd : WindowRoot
{
    public Animation tipsAni;
    public Text txtTips;
    public string TipsAniClipName = "TipsShowAni";


    private bool isTipsShow = false;
    private Queue<string> tipsQue = new Queue<string>();
    protected override void InitWnd()
    {
        base.InitWnd();

        SetActive(txtTips, false);
    }

    public void AddTips(string tips)
    {
        lock (tipsQue)
        {
            tipsQue.Enqueue(tips);
        }
    }

    private void Update()
    {
        if(tipsQue.Count > 0 && isTipsShow == false)
        {
            lock(tipsQue)
            {
                string tips = tipsQue.Dequeue();
                isTipsShow = true;
                SetTips(tips);
            }
        }
    }

    //显示Tips的接口
    private void SetTips(string tips)
    {
        SetActive(txtTips);
        SetText(txtTips, tips);

        AnimationClip clip = tipsAni.GetClip(TipsAniClipName);
        tipsAni.Play();
        //延时关闭激活状态

        StartCoroutine(AniPlayDone(clip.length, () =>
        {
            SetActive(txtTips, false);
            isTipsShow = false;
        }));
    }

    private IEnumerator AniPlayDone(float sec, Action cb)
    {
        yield return new WaitForSeconds(sec);
        if(cb != null)
        {
            cb();
        }
    }
}
