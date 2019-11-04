//Example script that displays the value of a timer in a scriptable object example.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleTimerUI : MonoBehaviour
{
    public Text timer; //the text ui object to display the timer.
    public FloatReference timerAsset; // the Scriptable object that stores the value.
    bool missingAssets = false;

    private void Start()
    {
        if(!timer)
        {
            Debug.Log("Timer Text Asset needs to be assigned.");
            missingAssets = true;
            return;
        }

        if (!timerAsset)
        {
            Debug.Log("Float Reference Scriptable Object needs to be assigned.");
            missingAssets = true;
            return;
        }

    }

    private void Update()
    {
        if (!missingAssets)
        {
            timer.text = timerAsset.value.ToString("F0");
        }
    }
}
