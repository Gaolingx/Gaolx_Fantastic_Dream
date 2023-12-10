using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//功能：动态UI元素界面
public class DynamicWnd : WindowRoot
{
    public Animation tipsAni;
    public Text txtTips;
    public string TipsAniClipName = "TipsShowAni";

    protected override void InitWnd()
    {
        base.InitWnd();

        SetActive(txtTips, false);
    }

    //显示Tips的接口
    public void SetTips(string tips)
    {
        SetActive(txtTips);
        SetText(txtTips, tips);

        AnimationClip clip = tipsAni.GetClip(TipsAniClipName);
        tipsAni.Play();
        //延时关闭激活状态

        StartCoroutine(AniPlayDone(clip.length, () =>
        {
            SetActive(txtTips, false);
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
