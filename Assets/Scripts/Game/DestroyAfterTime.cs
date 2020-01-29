﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that destroys the owning GameObject after a set time. The time can not be changed after the frame it is spawned.
/// </summary>
public class DestroyAfterTime : MonoBehaviour
{
    // Number of seconds before this object self-destructs
    public float secondsUntilDestroy = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("DestroyAfterSeconds", secondsUntilDestroy);
    }

    /// <summary>
    /// Despawns the object after the given time
    /// </summary>
    /// <param name="seconds">Number of seconds before despawn</param>
    IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        Destroy(gameObject);
    }
}