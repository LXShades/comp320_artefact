using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple tool for recording processing time
/// </summary>
public class Benchmark
{
    public static int currentIndex = 0;
        
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
    public static Benchmark New()
    {
        currentIndex = currentIndex + 1 & (poolSize - 1);
        pool[currentIndex].startTime = Time.realtimeSinceStartup;
        return pool[currentIndex];
    }

    /// <summary>
    /// Returns the number of milliseconds since this benchmark started
    /// </summary>
    public float ms
    {
        get
        {
            return (Time.realtimeSinceStartup - startTime) * 1000;
        }
    }

    /// <summary>
    /// Returns the number of seconds since this benchmark started
    /// </summary>
    public float seconds
    {
        get
        {
            return Time.realtimeSinceStartup - startTime;
        }
    }
}
