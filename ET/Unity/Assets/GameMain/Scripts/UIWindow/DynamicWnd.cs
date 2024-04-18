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
    public Transform hpItemRoot;

    private bool isTipsShow = false;
    private Queue<string> tipsQue = new Queue<string>();
    private Dictionary<string, ItemEntityHP> itemDic = new Dictionary<string, ItemEntityHP>();
    protected override void InitWnd()
    {
        base.InitWnd();

        SetActive(txtTips, false);
    }

    #region Tips相关
    public void AddTips(string tips)
    {
        lock (tipsQue)
        {
            tipsQue.Enqueue(tips);
        }
    }

    private void Update()
    {
        if (tipsQue.Count > 0 && isTipsShow == false)
        {
            lock (tipsQue)
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

        AnimationClip clip = tipsAni.GetClip(Constants.TipsAniClipName);
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
        if (cb != null)
        {
            cb();
        }
    }
    #endregion

    public void AddHpItemInfo(string mName, int hp)
    {
        ItemEntityHP item = null;
        if (itemDic.TryGetValue(mName, out item))
        {
            return;
        }
        else
        {
            //加载对应item，并放入ItemRoot下
            GameObject go = resSvc.LoadPrefab(PathDefine.HPItemPrefab, true);
            go.transform.SetParent(hpItemRoot);
            GameRoot.Instance.SetGameObjectTrans(go, new Vector3(-1000, 0, 0), Vector3.zero, Vector3.one); //默认设置在屏幕外
            ItemEntityHP ieh = go.GetComponent<ItemEntityHP>();
            ieh.SetItemInfo(hp); //将hp设置到Item中
            itemDic.Add(mName, ieh);
        }
    }
}
