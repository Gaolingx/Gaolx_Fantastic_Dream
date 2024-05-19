namespace DarkGod.Main
{
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

    public Animation selfDodgeAni;

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

        foreach (var item in itemDic.Values)
        {
            item.gameObject.SetActive(ShowItemEntityHPIfNeed(hpItemRoot));
        }
    }

    public bool ShowItemEntityHPIfNeed(Transform rootTrans)
    {
        Camera mainCamera = Camera.main;
        float scaleRate = 1.0f * Constants.ScreenStandardHeight / Screen.height;

        Vector3 fixedPos = Vector3.zero;
        //�������й����Transformӳ�����Ļ�ռ�����
        if (rootTrans != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(rootTrans.position);
            fixedPos = screenPos * scaleRate;
        }

        return UIItemUtils.IsMonsterOnScreen(fixedPos);
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

    public void AddHpItemInfo(string mName, Transform trans, int hp)
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
            GameRoot.Instance.SetGameObjectTrans(go, new Vector3(-1000, 0, 0), Vector3.zero, Vector3.one, true); //Ĭ����������Ļ��
            ItemEntityHP ieh = go.GetComponent<ItemEntityHP>();
            ieh.InitItemInfo(trans, hp); //��hp���õ�Item��
            itemDic.Add(mName, ieh);
        }
    }

    public void RmvHpItemInfo(string mName)
    {
        ItemEntityHP item = null;
        if (itemDic.TryGetValue(mName, out item))
        {
            //����Ѫ������
            Destroy(item.gameObject);
            //�Ƴ��ֵ�����
            itemDic.Remove(mName);
        }
    }

    public void SetDodge(string key)
    {
        ItemEntityHP item = null;
        if (itemDic.TryGetValue(key, out item))
        {
            item.SetDodge();
        }
    }

    public void SetCritical(string key, int critical)
    {
        ItemEntityHP item = null;
        if (itemDic.TryGetValue(key, out item))
        {
            item.SetCritical(critical);
        }
    }

    public void SetHurt(string key, int hurt)
    {
        ItemEntityHP item = null;
        if (itemDic.TryGetValue(key, out item))
        {
            item.SetHurt(hurt);
        }
    }

    public void SetHPVal(string key, int oldVal, int newVal)
    {
        ItemEntityHP item = null;
        if (itemDic.TryGetValue(key, out item))
        {
            item.SetHPVal(oldVal, newVal);
        }
    }

    public void SetSelfDodge()
    {
        selfDodgeAni.Stop();
        selfDodgeAni.Play();
    }
}

}