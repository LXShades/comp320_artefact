using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveExtensions
{
    /// <summary>
    /// Finds a time from a value in the given curve. This is iterative and may be slow.
    /// This assumes the value exists in the curve and also assumes the curve is monotonic.
    /// </summary>
    /// <param name="curve">Curve to inverse-evaluate</param>
    /// <param name="precision">The maximum amount of value error allowed</param>
    /// <returns></returns>
    public static float InverseEvaluate(this AnimationCurve curve, float value, float precision = 0.001f)
    {
        Keyframe[] keys = curve.keys;
        float currentTime = keys[0].time;
        float direction = Mathf.Sign(keys[keys.Length - 1].value - keys[0].value);
        float movementAmount = keys[keys.Length - 1].time - keys[0].time;

        // Cap the time value
        if (direction == 1)
        {
            if (value >= keys[keys.Length - 1].value)
            {
                return keys[keys.Length - 1].time - 0.001f;
            }
            else if (value <= keys[0].value)
            {
                return keys[0].time + 0.001f;
            }
        }
        else if (direction == -1)
        {
            if (value <= keys[keys.Length - 1].value)
            {
                return keys[keys.Length - 1].time - 0.001f;
            }
            else if (value >= keys[0].value)
            {
                return keys[0].time + 0.001f;
            }
        }
        
        for (int iteration = 0; iteration < 100; iteration++)
        {
            float currentValue = curve.Evaluate(currentTime);

            if (currentValue <= value + precision && currentValue >= value - precision)
            {
                return currentTime;
            }

            if ((currentValue < value) == (direction > 0))
            {
                // go along the current direction
                currentTime += movementAmount;
            }
            else
            {
                // go back, and go slower next time!
                currentTime -= movementAmount;
                movementAmount /= 2;
            }
        }

        // return the closest value we got (this generally shouldn't happen, perhaps precision is exceptionally low)
        return currentTime;
    }
}