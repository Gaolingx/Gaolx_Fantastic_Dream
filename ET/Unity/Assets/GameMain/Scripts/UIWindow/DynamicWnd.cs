//���ܣ���̬UIԪ�ؽ���
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

    #region Tips���
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

    //��ʾTips�Ľӿ�
    private void SetTips(string tips)
    {
        SetActive(txtTips);
        SetText(txtTips, tips);

        AnimationClip clip = tipsAni.GetClip(Constants.TipsAniClipName);
        tipsAni.Play();
        //��ʱ�رռ���״̬

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
            //���ض�Ӧitem��������ItemRoot��
            GameObject go = resSvc.LoadPrefab(PathDefine.HPItemPrefab, true);
            go.transform.SetParent(hpItemRoot);
            GameRoot.Instance.SetGameObjectTrans(go, new Vector3(-1000, 0, 0), Vector3.zero, Vector3.one); //Ĭ����������Ļ��
            ItemEntityHP ieh = go.GetComponent<ItemEntityHP>();
            ieh.SetItemInfo(hp); //��hp���õ�Item��
            itemDic.Add(mName, ieh);
        }
    }
}
