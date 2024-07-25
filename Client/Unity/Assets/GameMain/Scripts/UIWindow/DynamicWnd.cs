//���ܣ���̬UIԪ�ؽ���
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class DynamicWnd : WindowRoot
    {
        public Animation tipsAni;
        public Text txtTips;
        public Transform hpItemRoot;

        public Animation selfDodgeAni;

        private bool isTipsShow = false;
        private Queue<string> tipsQue = new Queue<string>();
        private Dictionary<string, Transform> monsterTransDic = new Dictionary<string, Transform>();
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
            foreach (var mName in monsterTransDic.Keys)
            {
                foreach (var item in itemDic.Values)
                {
                    if (item != null)
                    {
                        item.gameObject.SetActive(ShowItemEntityHPIfNeed(HasTrans(mName)));
                    }
                }
            }
        }

        private Transform HasTrans(string mName)
        {
            Transform transform = null;
            if (monsterTransDic.TryGetValue(mName, out transform))
            {
                return transform;
            }
            else
            {
                return null;
            }
        }

        public bool ShowItemEntityHPIfNeed(Transform rootTrans)
        {
            Camera mainCamera = Camera.main;
            Vector3 screenPos = Vector3.zero;
            if (rootTrans != null)
            {
                screenPos = mainCamera.WorldToScreenPoint(rootTrans.position);
            }

            return UIItemUtils.IsMonsterOnScreen(screenPos);
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

        public async void AddHpItemInfo(string mName, Transform trans, int hp)
        {
            ItemEntityHP item = null;
            if (itemDic.TryGetValue(mName, out item))
            {
                return;
            }
            else
            {
                //���ض�Ӧitem��������ItemRoot��
                GameObject go = await resSvc.LoadGameObjectAsync(Constants.ResourcePackgeName, PathDefine.HPItemPrefab, new Vector3(-1000, 0, 0), Vector3.zero, Vector3.one, true, true, true, transform.Find("hpItemRoot")); //Ĭ����������Ļ��
                go.transform.SetParent(hpItemRoot);
                ItemEntityHP ieh = go.GetComponent<ItemEntityHP>();
                ieh.InitItemInfo(trans, hp); //��hp���õ�Item��
                itemDic.Add(mName, ieh);
            }
            monsterTransDic.Add(mName, trans);
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
                monsterTransDic.Remove(mName);
            }
        }
        public void RmvAllHpItemInfo()
        {
            foreach (var item in itemDic)
            {
                Destroy(item.Value.gameObject);
            }
            itemDic.Clear();
            monsterTransDic.Clear();
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
