using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class DirectionVector2Utility
{
    private DirectionVector2Utility() { }

    /// <summary>
    /// �p�x��Vector2�ɕϊ�
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static Vector2 DirectionVector2(float angle)
    {
        var radian = angle * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }


}
