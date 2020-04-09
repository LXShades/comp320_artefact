using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Door that can be opened or closed by external triggers
/// </summary>
public class Door : MonoBehaviour
{
    /// <summary>
    /// Whether the door has already been opened and can't open again
    /// </summary>
    [HideInInspector] public bool hasOpened;

    /// <summary>
    /// Opens the door once
    /// </summary>
    public void Open()
    {
        if (!hasOpened)
        {
            GetComponent<Animation>()?.Play();

            hasOpened = true;

            if (!GameManager.singleton.hasTimerStarted)
            {
                GameManager.singleton.hasTimerStarted = true;
            }
        }
    }
}
