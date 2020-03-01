using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Captures frame statistics
/// </summary>
public class FpsSampler
{
    // How many samples should be taken per second
    public float sampleRate = 1f;

    // Last realtime that a sample was taken at
    private float lastSampleTime = 0;

    /// Samples deltaTime which should be an adequate measurement of frame times
    private List<float> deltaTimeSamples = new List<float>();

    public void Update()
    {
        if (Time.realtimeSinceStartup - lastSampleTime >= 1.0f / sampleRate)
        {
            deltaTimeSamples.Add(Time.deltaTime);
        }
    }

    /// <summary>
    /// Calculates and returns the average FPS recorded so far
    /// </summary>
    public float GetAverageFps()
    {
        if (deltaTimeSamples.Count == 0)
        {
            // Don't divide by 0
            return 0;
        }

        float average = 0;

        foreach (float sample in deltaTimeSamples)
        {
            average += sample;
        }

        return 1.0f / (average / deltaTimeSamples.Count);
    }

    /// <summary>
    /// Returns the FPS at the given percentile
    /// </summary>
    /// <param name="percentile">Percentile between 0-100</param>
    /// <returns></returns>
    public float GetFpsAtPercentile(float percentile = 1)
    {
        if (deltaTimeSamples.Count == 0)
        {
            return 0;
        }

        List<float> sortedFrameTimes = new List<float>(deltaTimeSamples);

        sortedFrameTimes.Sort();

        return sortedFrameTimes[(int)(percentile*0.01f * sortedFrameTimes.Count)];
    }

    /// <summary>
    /// Resets all recorded data
    /// </summary>
    public void Reset()
    {
        deltaTimeSamples.Clear();
        lastSampleTime = 0;
    }
}
