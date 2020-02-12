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

    void Start()
    {
        // Start with initial UI state
        completeScreen.SetActive(true);
        surveyScreen.SetActive(false);
        endScreen.SetActive(false);
    }

    void Update()
    {
        if (GameManager.singleton.timeRemaining > 0)
        {
            // Update player balloon status
            balloonStatusText.text = $"{GameManager.singleton.numPoppedBalloons}/{GameManager.singleton.numTotalBalloons}";
            timerText.text = $"{((int)GameManager.singleton.timeRemaining / 60).ToString("0.#")}:{((int)GameManager.singleton.timeRemaining % 60).ToString("00.#")}\nremaining";

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            endScreen.SetActive(true);
            Cursor.visible = true;

            Cursor.lockState = CursorLockMode.None;
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
