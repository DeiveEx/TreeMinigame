using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    /// <summary>
    /// Helper routine for animations. The animation body should expect a t value from 0 (start) to 1 (end)
    /// </summary>
    /// <param name="duration">The duration of the animation</param>
    /// <param name="animationBody">The actual implementation of the animation</param>
    public static IEnumerator AnimationRoutine(float duration, Action<float> animationBody, Action animationEndCallBack = null)
    {
        float timePassed = 0;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            animationBody(timePassed / duration);
            yield return null;
        }

        //To guarantee the animation finish at the correct state, we call the body with the end value (1)
        animationBody(1);
        animationEndCallBack?.Invoke();
    }
}
