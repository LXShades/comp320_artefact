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

    // End screen and survey
    public GameObject endScreen;
    // Round end screen
    public GameObject completeScreen;
    // Survey screen
    public GameObject surveyScreen;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.singleton.timeRemaining > 0)
        {
            // Update player balloon status
            balloonStatusText.text = $"Balloons: {GameManager.singleton.numTotalBalloons}\nPopped: {GameManager.singleton.numPoppedBalloons}";
            timerText.text = $"{((int)GameManager.singleton.timeRemaining / 60).ToString("0.#")}:{((int)GameManager.singleton.timeRemaining % 60).ToString("00.#")}\nremaining";
        }
        else
        {
            endScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Called when the button to begin the survey is clicked
    /// </summary>
    public void OnClickedBeginSurvey()
    {
        completeScreen.SetActive(false);
        surveyScreen.SetActive(true);
    }
}
