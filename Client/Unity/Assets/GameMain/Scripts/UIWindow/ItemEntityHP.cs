namespace DarkGod.Main
{
//���ܣ�Ѫ������

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemEntityHP : MonoBehaviour
{
    #region UI Define
    public Image imgHPGray;
    public Image imgHPRed; //ʵ��Ѫ��

    public Animation criticalAni;
    public Text txtCritical;

    public Animation dodgeAni;
    public Text txtDodge;

    public Animation hpAni;
    public Text txtHp;

    public float SPvalOffset = 0f;
    #endregion

    private RectTransform rect;
    private Transform rootTrans; //�����Transform
    private int hpVal;

    private float scaleRate = 1.0f * Constants.ScreenStandardHeight / Screen.height;

    Vector3 screenPos;
    private void Update()
    {
        //�������й����Transformӳ�����Ļ�ռ�����
        if (rootTrans != null)
        {
            screenPos = Camera.main.WorldToScreenPoint(rootTrans.position);
            rect.anchoredPosition = screenPos * scaleRate;
        }

        currentPrg = UIItemUtils.UpdateMixBlend(currentPrg, targetPrg, Constants.AccelerHPSpeed, SPvalOffset);
        imgHPGray.fillAmount = currentPrg;

    }

    public void InitItemInfo(Transform trans, int hp)
    {
        rect = transform.GetComponent<RectTransform>();
        rootTrans = trans;
        hpVal = hp; //��Ѫ��
        imgHPGray.fillAmount = 1;
        imgHPRed.fillAmount = 1;
    }

    public void SetCritical(int critical)
    {
        criticalAni.Stop();
        txtCritical.text = "���� " + critical;
        criticalAni.Play();
    }

    public void SetDodge()
    {
        dodgeAni.Stop();
        txtDodge.text = "����";
        dodgeAni.Play();
    }

    public void SetHurt(int hurt)
    {
        hpAni.Stop();
        txtHp.text = "-" + hurt;
        hpAni.Play();
    }

    private float currentPrg;
    private float targetPrg;
    public void SetHPVal(int oldVal, int newVal)
    {
        //����Ѫ���仯
        currentPrg = oldVal * 1.0f / hpVal;
        targetPrg = newVal * 1.0f / hpVal;
        //����Ŀ��Ѫ��
        imgHPRed.fillAmount = targetPrg;
        //�������䶯��
    }
}

}