using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple tool for recording processing time
/// </summary>
public class Benchmark
{
    /// <summary>
    /// Index of the next available benchmark in the preallocated benchmark pool
    /// </summary>
    public static int currentIndex = 0;

    /// <summary>
    /// If the benchmark has been stopped, this is the realTime at which the stop happened
    /// </summary>
    private float timeAtStop = 0f;

    /// <summary>
    /// Max size of the benchmark pool. Must be a power of 2.
    /// </summary>
    public const int poolSize = 16;

    /// <summary>
    /// Pool of benchmark objects. These are pre-created to avoid allocation slowdown.
    /// </summary>
    public static Benchmark[] pool = new Benchmark[poolSize];

    /// <summary>
    /// Realtime that this benchmark started
    /// </summary>
    public float startTime;

    /// <summary>
    /// Preallocates the benchmark pool
    /// </summary>
    static Benchmark()
    {
        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = new Benchmark();
        }
    }

    /// <summary>
    /// Returns a new benchmarking object
    /// </summary>
    /// <returns>A benchmarking object starting at this time</returns>
    public static Benchmark Start()
    {
        currentIndex = currentIndex + 1 & (poolSize - 1);
        pool[currentIndex].startTime = Time.realtimeSinceStartup;
        pool[currentIndex].timeAtStop = 0.0f;
        return pool[currentIndex];
    }

    /// <summary>
    /// Ends the bencharking period for this benchmark
    /// </summary>
    public void Stop()
    {
        if (timeAtStop == 0f)
        {
            timeAtStop = Time.realtimeSinceStartup;
        }
    }

    /// <summary>
    /// Returns the number of milliseconds since this benchmark started
    /// </summary>
    public float ms
    {
        get
        {
            if (timeAtStop == 0.0f)
            {
                return (Time.realtimeSinceStartup - startTime) * 1000;
            }
            else
            {
                return (timeAtStop - startTime) * 1000;
            }
        }
    }

    /// <summary>
    /// Returns the number of seconds since this benchmark started
    /// </summary>
    public float seconds
    {
        get
        {
            if (timeAtStop == 0.0f)
            {
                return Time.realtimeSinceStartup - startTime;
            }
            else
            {
                return timeAtStop - startTime;
            }
        }
    }
}
