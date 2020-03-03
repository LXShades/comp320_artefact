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

    // In-game crosshair
    public GameObject crosshair;
    // End screen and survey
    public GameObject endScreen;
    // Round end screen
    public GameObject completeScreen;
    // Survey screen
    public GameObject surveyScreen;
    // Tutorial screen, shows until the game starts
    public GameObject tutorialScreen;
    // Debug UI
    public GameObject debugUi;

    void Start()
    {
        // Start with initial UI state
        completeScreen.SetActive(true);
        surveyScreen.SetActive(false);
        endScreen.SetActive(false);

        if (GameManager.isDebugBuild && debugUi)
        {
            debugUi.SetActive(true);
        }
    }

    void Update()
    {
        tutorialScreen.SetActive(!GameManager.singleton.hasTimerStarted);

        if (GameManager.singleton.timeRemaining > 0)
        {
            // Update player balloon status
            balloonStatusText.text = $"{GameManager.singleton.numPoppedBalloons}/{GameManager.singleton.numTotalBalloons}";

            if (GameManager.singleton.hasTimerStarted)
            {
                timerText.text = $"{((int)GameManager.singleton.timeRemaining / 60).ToString("0.#")}:{((int)GameManager.singleton.timeRemaining % 60).ToString("00.#")}\nremaining";
            }
            else
            {
                timerText.text = "Waiting for door...";
            }

            crosshair.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            endScreen.SetActive(true);
            Cursor.visible = true;

            crosshair.SetActive(false);

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
