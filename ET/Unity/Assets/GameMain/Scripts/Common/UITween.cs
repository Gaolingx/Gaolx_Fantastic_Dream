//功能：UI过渡混合工具

using UnityEngine;

public static class UITween
{
    public static float UpdateMixBlend(float currentPrg, float targetPrg, float accelerHPSpeed, float accelerOffset = 0f)
    {
        if (Mathf.Abs(currentPrg - targetPrg) < (accelerHPSpeed + accelerOffset) * Time.deltaTime)
        {
            currentPrg = targetPrg;
        }
        else if (currentPrg > targetPrg)
        {
            currentPrg -= (accelerHPSpeed + accelerOffset) * Time.deltaTime;
        }
        else
        {
            currentPrg += (accelerHPSpeed + accelerOffset) * Time.deltaTime;
        }
        return currentPrg;
    }
}

