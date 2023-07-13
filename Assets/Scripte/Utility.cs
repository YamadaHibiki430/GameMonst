using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Utility
{
    private Utility() { }

    /// <summary>
    /// �����_�̑�N�ȉ���؂�̂�
    /// </summary>
    /// <param name="value"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public static float TruncateNumbers(float value,int n)
    {
        float powValue = Mathf.Pow(10, n);
        return Mathf.Floor(value * powValue) / powValue;
    }
}
