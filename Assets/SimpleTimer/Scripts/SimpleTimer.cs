//Simple Timer script that uses a scriptable object to hold the value of time.

using UnityEngine;

public class SimpleTimer : MonoBehaviour
{
    public float startTime = 60f; // The time to set the timer to count down from.
    public FloatReference timerAsset; //the Scriptable object that holds the current value of the timer.
    public bool timerActive = true; //use this to pause the timer
    bool timeUp = false; // put this in so the timer only triggers once

    private void Start()
    {
        //make sure the timer asset has been assigned.
        if(!timerAsset)
        {
            Debug.LogError("Need to assign a timer asset");
            timerActive = false;
            return;
        }
        timerAsset.value = startTime;
        timeUp = false;
    }

    private void Update()
    {
        if (timerActive && !timeUp)
        {
            timerAsset.value -= Time.deltaTime;
            if (timerAsset.value <= 0)
            {
                Debug.Log("Time is UP!");
                timeUp = true;
            }
        }
    }

    public bool TimeUp()
    {
        return timeUp;
    }
}
