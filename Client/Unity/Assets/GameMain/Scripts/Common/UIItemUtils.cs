//功能：UI工具类
using UnityEngine;

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

    /// <summary>
    /// 2.获得交点
    /// </summary>
    /// <param name="enemyIndicator"></param>
    /// <param name="target"></param>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <param name="edgeOffset"></param>
    public static void OnLinearAlgebra(Transform enemyIndicator, Vector3 target, Vector3 pos1, Vector3 pos2, float edgeOffset)
    {
        // 定义屏幕边界
        Vector2 la = new Vector2(edgeOffset, edgeOffset); //左下
        Vector2 lb = new Vector2(edgeOffset, Screen.height - edgeOffset); //左上
        Vector2 ra = new Vector2(Screen.width - edgeOffset, edgeOffset); //右下
        Vector2 rb = new Vector2(Screen.width - edgeOffset, Screen.height - edgeOffset); //右上

        // 检查与每条边界的交点
        if (GetPoint(out target, pos1, pos2, la, lb)) //左边界
            enemyIndicator.transform.position = target;
        else if (GetPoint(out target, pos1, pos2, ra, rb)) //右边界
            enemyIndicator.transform.position = target;
        else if (GetPoint(out target, pos1, pos2, lb, rb)) //上边界
            enemyIndicator.transform.position = target;
        else if (GetPoint(out target, pos1, pos2, la, ra)) //下边界
            enemyIndicator.transform.position = target;
    }

    public static bool GetPoint(out Vector3 target, Vector2 pos1, Vector2 pos2, Vector2 pos3, Vector2 pos4)
    {
        target = default;
        float a = pos2.y - pos1.y;
        float b = pos1.x - pos2.x;
        float c = pos2.x * pos1.y - pos1.x * pos2.y;
        float d = pos4.y - pos3.y;
        float e = pos3.x - pos4.x;
        float f = pos4.x * pos3.y - pos3.x * pos4.y;
        float denominator = a * e - d * b;
        if (Mathf.Abs(denominator) < Mathf.Epsilon) //平行
            return false;

        float x = (f * b - c * e) / denominator;
        float y = (c * d - f * a) / denominator;

        if (x < 0 || y < 0 || x > Screen.width || y > Screen.height)
            return false;

        if (!GetOnLine(pos1, pos2, new Vector2(x, y)))
            return false;

        target = new Vector3(x, y, 0); //z设为0，因为这是屏幕坐标
        return true;
    }

    public static bool GetOnLine(Vector2 pos1, Vector2 pos2, Vector2 pos3)
    {
        float EPS = 1e-3f; //误差
        float d1 = Vector2.Distance(pos1, pos3);
        float d2 = Vector2.Distance(pos2, pos3);
        float d3 = Vector2.Distance(pos1, pos2);
        return Mathf.Abs(d1 + d2 - d3) <= EPS;
    }

    public static void UILookAt(Transform ctrlObj, Vector3 dir, Vector3 lookAxis)
    {
        Quaternion q = Quaternion.identity;
        q.SetFromToRotation(lookAxis, dir);
        ctrlObj.eulerAngles = new Vector3(q.eulerAngles.x, 0, q.eulerAngles.z);
    }

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