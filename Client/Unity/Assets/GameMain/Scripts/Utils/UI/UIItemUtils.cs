//功能：UI工具类

using UnityEngine;

namespace DarkGod.Tools
{
    public static class UIItemUtils
    {
        /// <summary>
        /// 1.设置游戏对象位置
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="localScale"></param>
        /// <param name="isLocalPos"></param>
        /// <param name="isLocalRotation"></param>
        /// <param name="isSetParent"></param>
        /// <param name="rootTrans"></param>
        /// <param name="gameObjectName"></param>
        /// <returns></returns>
        public static Transform SetGameObjectTrans(GameObject gameObject, Vector3 position, Quaternion rotation, Vector3 localScale, bool isLocalPos, bool isLocalRotation, bool isSetParent = false, Transform rootTrans = null, string gameObjectName = default)
        {
            if (gameObjectName != default)
            {
                gameObject.name = gameObjectName;
            }

            if (isSetParent)
            {
                gameObject.transform.SetParent(rootTrans);
            }

            if (isLocalPos)
            {
                gameObject.transform.localPosition = position;
            }
            else
            {
                gameObject.transform.position = position;
            }

            if (isLocalRotation)
            {
                gameObject.transform.localRotation = rotation;
            }
            else
            {
                gameObject.transform.localRotation = rotation;
            }

            gameObject.transform.localScale = localScale;

            return gameObject.transform;
        }

        /// <summary>
        /// 2.判断怪物是否在屏幕内
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


        /// <summary>
        /// Tween相关
        /// </summary>
        /// <param name="currentPrg"></param>
        /// <param name="targetPrg"></param>
        /// <param name="accelerHPSpeed"></param>
        /// <param name="accelerOffset"></param>
        /// <returns></returns>
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

        public static void UpdateMixBlendAnim(float currentBlend, float targetBlend, float accelerSpeed, GameObject obj, string propName)
        {
            if (Mathf.Abs(currentBlend - targetBlend) < accelerSpeed * Time.deltaTime)
            {
                currentBlend = targetBlend;
            }
            else if (currentBlend > targetBlend)
            {
                currentBlend -= accelerSpeed * Time.deltaTime;
            }
            else
            {
                currentBlend += accelerSpeed * Time.deltaTime;
            }
            obj.GetComponent<Animator>().SetFloat(propName, currentBlend);
        }

        /// <summary>
        /// 类型转换相关
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
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

        /// <summary>
        /// ScreenScale 获取
        /// </summary>
        /// <returns>Vector2(宽度, 高度)</returns>
        public static Vector2 GetScreenScale(float screenStandardWidth, float screenStandardHeight)
        {
            return new Vector2(1.0f * screenStandardWidth / Screen.width, 1.0f * screenStandardHeight / Screen.height);
        }
    }
}