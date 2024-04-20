//功能：血条物体

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemEntityHP : MonoBehaviour
{
    #region UI Define
    public Image imgHPGray;
    public Image imgHPRed; //实际血量

    public Animation criticalAni;
    public Text txtCritical;

    public Animation dodgeAni;
    public Text txtDodge;

    public Animation hpAni;
    public Text txtHp;

    public float SPvalOffset = 0f;
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

        currentPrg = UITween.UpdateMixBlend(currentPrg, targetPrg, Constants.AccelerHPSpeed, SPvalOffset);
        imgHPGray.fillAmount = currentPrg;
    }

    public void InitItemInfo(Transform trans, int hp)
    {
        rect = transform.GetComponent<RectTransform>();
        rootTrans = trans;
        hpVal = hp; //总血量
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

    private float currentPrg;
    private float targetPrg;
    public void SetHPVal(int oldVal, int newVal)
    {
        //计算血量变化
        currentPrg = oldVal * 1.0f / hpVal;
        targetPrg = newVal * 1.0f / hpVal;
        //设置目标血量
        imgHPRed.fillAmount = targetPrg;
        //产生渐变动画
    }
}
