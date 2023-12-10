using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//���ܣ���̬UIԪ�ؽ���
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

    //��ʾTips�Ľӿ�
    public void SetTips(string tips)
    {
        SetActive(txtTips);
        SetText(txtTips, tips);

        AnimationClip clip = tipsAni.GetClip(TipsAniClipName);
        tipsAni.Play();
        //��ʱ�رռ���״̬

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
