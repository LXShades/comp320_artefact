using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player UI handler & refresher
/// </summary>
public class PlayerUI : MonoBehaviour
{
    // UI text for the balloon status info
    public Text balloonStatusText;
    // UI text for the time remaining
    public Text timerText;

    // Update is called once per frame
    void Update()
    {
        // Update player balloon status
        balloonStatusText.text = $"Balloons: {GameManager.singleton.numTotalBalloons}\nPopped: {GameManager.singleton.numPoppedBalloons}";
        timerText.text = $"{GameManager.singleton.timeRemaining}\nremaining";
    }
}
