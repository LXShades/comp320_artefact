using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTests : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestCurveExtensions();
    }

    void TestCurveExtensions()
    {
        AnimationCurve upCurve = AnimationCurve.Linear(0, 1, 5, 2);
        AnimationCurve downCurve = AnimationCurve.Linear(0, 5, 2, 0);
        float precision = 0.05f;

        RunTest("UpCurve", upCurve.InverseEvaluate(1.5f, precision), 2.5f, precision);
        RunTest("DownCurve", downCurve.InverseEvaluate(2.5f, precision), 1f, precision);
    }

    bool RunTest(string testName, float actualValue, float expectedValue, float precision)
    {
        if (actualValue - expectedValue > precision || actualValue - expectedValue < -precision)
        {
            Debug.LogWarning($"Test {testName} failed (value: {actualValue} expected: {expectedValue})");
            return false;
        }

        Debug.Log($"Test {testName} passed (value: {actualValue} expected: {expectedValue})");
        return true;
    }
}
