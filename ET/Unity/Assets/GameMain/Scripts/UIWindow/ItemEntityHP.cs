//���ܣ�Ѫ������

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemEntityHP : MonoBehaviour
{
    #region UI Define
    public Image imgHPGray;
    public Image imgHPRed;

    public Animation criticalAni;
    public Text txtCritical;

    public Animation dodgeAni;
    public Text txtDodge;

    public Animation hpAni;
    public Text txtHp;
    #endregion

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
}
