using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 扩展方法
/// </summary>`
public static class ExtensionMethod
{
    private const float dotThreshold = 0.5f;
    public static bool IsFacingTarget(this Transform transform, Transform target)
    {
        var VectorToTarget = target.position - transform.position; //玩家到敌人的向量
        VectorToTarget.Normalize(); //单位向量化

        float dot = Vector3.Dot(transform.forward, VectorToTarget); //求点积，判断是否面对目标

        return dot >= dotThreshold; //cos(夹角)>= 0.5 即夹角小于60度，玩家位于敌人面前角度为120°的扇区，认为面对目标
    }
}
