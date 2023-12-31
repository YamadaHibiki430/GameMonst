using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Utility
{
    private Utility() { }

    /// <summary>
    /// 小数点の第N以下を切り捨て
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
