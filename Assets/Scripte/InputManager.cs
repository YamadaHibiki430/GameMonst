using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : SingletonMonoBehaviour<InputManager>
{
    private Vector2 touchStartPos = Vector2.zero;
    private Vector2 endTouchPos = Vector2.zero;

    /// <summary>
    /// 画面をタッチした後にスライドしたときの角度
    /// </summary>
    /// <returns></returns>
    public Vector2 TouchPullDirection()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            touchStartPos = Input.mousePosition;
            endTouchPos = Input.mousePosition;
        }
        if (Input.GetKey(KeyCode.Mouse0))
        {
            endTouchPos = Input.mousePosition;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            endTouchPos = Input.mousePosition;
            Vector2 result = touchStartPos * 10 - endTouchPos * 10;
            return result.normalized;
        }
        return Vector2.zero;
    }


}
