using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performs unit tests for debugging
/// </summary>
public class UnitTests : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestCurveExtensions();
        TestDataFile();
    }

    void TestCurveExtensions()
    {
        AnimationCurve upCurve = AnimationCurve.Linear(0, 1, 5, 2);
        AnimationCurve downCurve = AnimationCurve.Linear(0, 5, 2, 0);
        float precision = 0.05f;

        RunTest("UpCurve", upCurve.InverseEvaluate(1.5f, precision), 2.5f, precision);
        RunTest("DownCurve", downCurve.InverseEvaluate(2.5f, precision), 1f, precision);
    }

    /// <summary>
    /// Formatting tests for data files
    /// </summary>
    void TestDataFile()
    {
        DataFile file = new DataFile($"{Application.dataPath}/CSVTest.csv");

        file.sessionData["impostorsA"] = "didn't notice";
        file.sessionData["impostorsB"] = "kinda noticed";
        file.sessionData["impostorsC"] = "pretty obvious mate";
        file.sessionData["impostorsD"] = "missed it like it was the battle of britain";

        RunTest("CreateDataFormat", file.CreateDataFormat(), new string[] { "impostorsA", "impostorsB", "impostorsC", "impostorsD" });
        file.WriteToFile();
        RunTest("GetDataFormat", file.GetDataFormat(file.filename), new string[] { "impostorsA", "impostorsB", "impostorsC", "impostorsD" });
    }

    /// <summary>
    /// Runs a float test and returns whether passed
    /// </summary>
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

    /// <summary>
    /// Runs a string array test and returns whether passed
    /// </summary>
    bool RunTest(string testName, string[] actualValue, string[] expectedValue)
    {
        for (int i = 0, end = Mathf.Max(actualValue.Length, expectedValue.Length); i < end; i++)
        {
            if (actualValue[i] != expectedValue[i])
            {
                foreach (var a in actualValue[i].ToCharArray())
                {
                    Debug.Log(a);
                }
                Debug.LogWarning($"Test {testName} failed at index {i}: (value: {actualValue[i]} expected: {expectedValue[i]})");
                return false;
            }
        }

        if (actualValue.Length != expectedValue.Length)
        {
            Debug.LogWarning($"Test {testName} failed: Values equal, lengths different (actual: {actualValue.Length} expected: {expectedValue.Length})");
            return false;
        }

        Debug.Log($"Test {testName} passed");
        return true;
    }
}
