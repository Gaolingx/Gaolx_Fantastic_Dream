using UnityEngine;
using System.Collections.Generic;
using System;

public static partial class Geometry2DUtility
{
    /// <summary>
    /// 计算两点间距离
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <returns></returns>
    public static float CalculatePointDistance(Vector2 point1, Vector2 point2)
    {
        float distance = (float)Math.Sqrt(Math.Pow((point2.x - point1.x), 2) + Math.Pow((point2.y - point1.y), 2));
        return distance;
    }

    /// <summary>
    /// 计算点到直线距离
    /// </summary>
    /// <param name="p"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static float CalculateDistanceFromPoint2Line(Vector3 p, Vector3 p1, Vector3 p2)
    {
        // 求A2B的距离
        float p2pDistance = Vector3.Distance(p2, p); // 或者使用 p2p.magnitude
                                                     // p1->p2的向量
        Vector3 p2p1 = p2 - p1;
        Vector3 p2p = p2 - p;
        // 求p2p1·p2p
        float dotResult = Vector3.Dot(p2p1, p2p);
        // 求θ
        float seitaRad = Mathf.Acos(dotResult / (p2p1.magnitude * p2pDistance));
        // 求p点到p1p2的距离
        float distance = p2pDistance * Mathf.Sin(seitaRad);
        return distance;
    }

    /// <summary>
    /// 判定点是否在圆内
    /// </summary>
    /// <param name="circlePosition"></param>
    /// <param name="circleRadius"></param>
    /// <param name="point"></param>
    public static bool IsInCircle(Vector3 circlePosition, float circleRadius,
        Vector3 point)
    {
        return IsCirclesIntersecting(circlePosition, circleRadius, point, 0);
    }

    /// <summary>
    /// 判定两个圆是否相交
    /// </summary>
    /// <param name="circlePosition1"></param>
    /// <param name="circleRadius1"></param>
    /// <param name="circlePosition2"></param>
    /// <param name="circleRadius2"></param>
    public static bool IsCirclesIntersecting(
        Vector3 circlePosition1, float circleRadius1,
        Vector3 circlePosition2, float circleRadius2)
    {
        if (circleRadius1 < 0 || circleRadius2 < 0)
        {
            return false;
        }

        var diff = circlePosition2 - circlePosition1;
        var r = circleRadius1 + circleRadius2;
        if (Vector3.SqrMagnitude(diff) < r * r)
        {
            return true;
        }

        return false;
    }

    public static bool IsInSector(
        Vector3 sectorPosition, Vector3 sectorForward,
        float sectorRadius, float sectorAngle,
        Vector3 point)
    {
        return IsInAnnularSector(sectorPosition, sectorForward, sectorRadius, 0, sectorAngle, point);
    }

    public static bool IsInSector(
        Vector3 sectorPosition, Vector3 sectorForward,
        float sectorRadius, float sectorAngle,
        Vector3 circlePosition, float circleRadius)
    {
        return IsInAnnularSector(sectorPosition, sectorForward, sectorRadius, 0, sectorAngle, circlePosition, circleRadius);
    }

    /// <summary>
    /// 判定点是否在环形扇区内
    /// </summary>
    /// <param name="sectorPosition">扇区起点</param>
    /// <param name="sectorForward">扇区朝向(单位向量)</param>
    /// <param name="sectorOuterRadius">扇区外半径</param>
    /// <param name="sectorInnerRadius">扇区内半径, 为0时为标准扇形</param>
    /// <param name="sectorAngle">扇区角度</param>
    /// <param name="point">点</param>
    public static bool IsInAnnularSector(
        Vector3 sectorPosition, Vector3 sectorForward,
        float sectorOuterRadius, float sectorInnerRadius,
        float sectorAngle,
        Vector3 point)
    {
        sectorInnerRadius = Mathf.Clamp(sectorInnerRadius, 0, sectorOuterRadius);

        var directionToPoint = point - sectorPosition;
        var sqrDistanceToPoint = directionToPoint.sqrMagnitude;
        if (sqrDistanceToPoint < sectorOuterRadius * sectorOuterRadius &&
            sqrDistanceToPoint >= sectorInnerRadius * sectorInnerRadius)
        {
            var angleToPoint = Vector3.Angle(sectorForward, directionToPoint);
            if (angleToPoint < sectorAngle * 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判定圆是否在环形扇区内
    /// </summary>
    /// <param name="sectorPosition">扇区起点</param>
    /// <param name="sectorForward">扇区朝向(单位向量)</param>
    /// <param name="sectorOuterRadius">扇区外半径</param>
    /// <param name="sectorInnerRadius">扇区内半径, 为0时为标准扇形</param>
    /// <param name="sectorAngle">扇区角度</param>
    /// <param name="circlePosition">圆心位置</param>
    /// <param name="circleRadius">圆的半径</param>
    public static bool IsInAnnularSector(
        Vector3 sectorPosition, Vector3 sectorForward,
        float sectorOuterRadius, float sectorInnerRadius,
        float sectorAngle,
        Vector3 circlePosition, float circleRadius)
    {
        if (sectorOuterRadius <= 0 || sectorAngle <= 0)
        {
            return false;
        }

        sectorInnerRadius = Mathf.Clamp(sectorInnerRadius, 0, sectorOuterRadius);

        // 检查圆心是否在扇区内
        if (IsInAnnularSector(sectorPosition, sectorForward, sectorOuterRadius + circleRadius, sectorInnerRadius - circleRadius, sectorAngle,
                circlePosition))
        {
            return true;
        }

        // 检查圆是否与扇区边界相交
        if (circleRadius > 0)
        {
            var halfSectorAngle = sectorAngle * 0.5f;

            // 左边界
            var leftBoundaryPoints = CalculateBoundaryPoints(sectorPosition, sectorForward, sectorInnerRadius, sectorOuterRadius, -halfSectorAngle);
            if (IsCircleIntersectingLine(circlePosition, circleRadius, leftBoundaryPoints.boundaryInnerPoint, leftBoundaryPoints.boundaryOuterPoint))
            {
                return true;
            }

            // 右边界
            var rightBoundaryPoints = CalculateBoundaryPoints(sectorPosition, sectorForward, sectorInnerRadius, sectorOuterRadius, halfSectorAngle);
            if (IsCircleIntersectingLine(circlePosition, circleRadius, rightBoundaryPoints.boundaryInnerPoint, rightBoundaryPoints.boundaryOuterPoint))
            {
                return true;
            }
        }

        return false;

        static (Vector3 boundaryInnerPoint, Vector3 boundaryOuterPoint)
            CalculateBoundaryPoints(Vector3 sectorPosition, Vector3 sectorForward,
                float sectorInnerRadius, float sectorOuterRadius,
                float angle)
        {
            var boundaryDirection = Quaternion.Euler(0, angle, 0) * sectorForward;
            var boundaryInnerPoint = sectorPosition + boundaryDirection * sectorInnerRadius;
            var boundaryOuterPoint = sectorPosition + boundaryDirection * sectorOuterRadius;
            return (boundaryInnerPoint, boundaryOuterPoint);
        }
    }


    /// <summary>
    /// 判定圆是否与线段相交
    /// </summary>
    /// <param name="circlePosition"></param>
    /// <param name="circleRadius"></param>
    /// <param name="lineStart"></param>
    /// <param name="lineEnd"></param>
    public static bool IsCircleIntersectingLine(Vector3 circlePosition, float circleRadius, Vector3 lineStart,
        Vector3 lineEnd)
    {
        var lineDir = lineEnd - lineStart;
        var toCircle = circlePosition - lineStart;
        var projectionLength = Vector3.Dot(toCircle, lineDir.normalized);
        var closestPoint = lineStart + lineDir.normalized * Mathf.Clamp(projectionLength, 0, lineDir.magnitude);
        var sqrDistanceToLine = Vector3.SqrMagnitude(circlePosition - closestPoint);
        return sqrDistanceToLine < circleRadius * circleRadius;
    }

    /// <summary>
    /// 判定点是否在基准位置的前方矩形区域内
    /// </summary>
    /// <remarks>比如英雄联盟中的剑姬W技能(基准位置为剑姬, 基准朝向为剑姬朝向)</remarks>
    /// <param name="position">基准位置</param>
    /// <param name="forward">基准朝向(单位向量)</param>
    /// <param name="rectWidth">矩形宽度(左右)</param>
    /// <param name="rectLength">矩形长度(前后)</param>
    /// <param name="point">要判定的点</param>
    public static bool IsInFrontRectangle(Vector3 position, Vector3 forward,
        float rectWidth, float rectLength,
        Vector3 point)
    {
        var direction = point - position;
        var dot = Vector3.Dot(forward, direction);

        if (dot > 0)
        {
            var forwardProject = Vector3.Project(direction, forward).magnitude;
            if (forwardProject < rectLength)
            {
                var right = Vector3.Cross(forward, Vector3.up);
                var rightProject = Vector3.Project(direction, right).magnitude;
                if (Mathf.Abs(rightProject) <= rectWidth * .5f)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
