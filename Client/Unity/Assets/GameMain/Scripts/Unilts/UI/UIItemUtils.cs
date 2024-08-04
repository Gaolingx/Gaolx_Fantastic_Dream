//功能：UI工具类
using System;
using UnityEngine;

namespace DarkGod.Main
{
    public static class UIItemUtils
    {
        /// <summary>
        /// 1.判断怪物是否在屏幕内
        /// </summary>
        /// <param name="monsterScreenPos"></param>
        /// <returns></returns>
        public static bool IsMonsterOnScreen(Vector3 monsterScreenPos)
        {
            return monsterScreenPos.x < Screen.width && monsterScreenPos.y < Screen.height && monsterScreenPos.x > 0 && monsterScreenPos.y > 0 && monsterScreenPos.z > 0;
        }

        public static void UILookAt(Transform ctrlObj, Vector3 dir, Vector3 lookAxis)
        {
            Quaternion q = Quaternion.identity;
            q.SetFromToRotation(lookAxis, dir);
            ctrlObj.eulerAngles = new Vector3(q.eulerAngles.x, 0, q.eulerAngles.z);
        }


        // Tween相关
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

        public static void UpdateMixBlendAnim(float currentBlend, float targetBlend, GameObject obj, string propName)
        {
            if (Mathf.Abs(currentBlend - targetBlend) < Constants.AccelerSpeed * Time.deltaTime)
            {
                currentBlend = targetBlend;
            }
            else if (currentBlend > targetBlend)
            {
                currentBlend -= Constants.AccelerSpeed * Time.deltaTime;
            }
            else
            {
                currentBlend += Constants.AccelerSpeed * Time.deltaTime;
            }
            obj.GetComponent<Animator>().SetFloat(propName, currentBlend);
        }

        // 类型转换相关
        public static int BoolToInt(bool val)
        {
            if (val)
                return 1;
            else
                return 0;
        }

        public static bool IntToBool(int val)
        {
            if (val != 0)
                return true;
            else
                return false;
        }

        public static float SetAudioVolumeVal(float targetVolume)
        {
            float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1f);
            return Mathf.Log10(targetValue) * 20;
        }
    }
}