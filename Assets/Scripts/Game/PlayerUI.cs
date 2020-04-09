using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player UI handler & refresher
/// </summary>
public class PlayerUI : MonoBehaviour
{
    [Tooltip("UI text for the balloon status info")]
    public Text balloonStatusText;
    [Tooltip("UI text for the time remaining")]
    public Text timerText;

    [Tooltip("In-game crosshair")]
    public GameObject crosshair;
    [Tooltip("End screen and survey")]
    public GameObject endScreen;
    [Tooltip("Round end screen")]
    public GameObject completeScreen;
    [Tooltip("Survey screen")]
    public GameObject surveyScreen;
    [Tooltip("Tutorial screen, shows until the game starts")]
    public GameObject tutorialScreen;
    [Tooltip("Debug UI")]
    public GameObject debugUi;

    /// <summary>
    /// Called by Unity upon creation. Initialises UI elements
    /// </summary>
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

    /// <summary>
    /// Called by Unity upon a frame. Refreshes UI display, cursor control  and stats
    /// </summary>
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

            // Lock the cursor (we're in game)
            crosshair.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Unlock the cursor (we're doing the survey or end screen now)
            endScreen.SetActive(true);
            crosshair.SetActive(false);

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
