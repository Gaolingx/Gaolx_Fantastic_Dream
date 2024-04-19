//功能：血条物体

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

    private RectTransform rect;
    private Transform rootTrans; //怪物的Transform
    private int hpVal;

    private float scaleRate = 1.0f * Constants.ScreenStandardHeight / Screen.height;

    private void Update()
    {
        //将场景中怪物的Transform映射成屏幕空间坐标
        if (rootTrans != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(rootTrans.position);
            rect.anchoredPosition = screenPos * scaleRate;
        }
    }

    public void InitItemInfo(Transform trans, int hp)
    {
        rect = transform.GetComponent<RectTransform>();
        rootTrans = trans;
        hpVal = hp;
        imgHPGray.fillAmount = 1;
        imgHPRed.fillAmount = 1;
    }

    public void SetCritical(int critical)
    {
        criticalAni.Stop();
        txtCritical.text = "暴击 " + critical;
        criticalAni.Play();
    }
    public void SetDodge()
    {
        dodgeAni.Stop();
        txtDodge.text = "闪避";
        dodgeAni.Play();
    }
    public void SetHurt(int hurt)
    {
        hpAni.Stop();
        txtHp.text = "-" + hurt;
        hpAni.Play();
    }
}
