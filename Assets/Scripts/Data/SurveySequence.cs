using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Progresses through the survey UI with a series of questions
/// </summary>
public class SurveySequence : MonoBehaviour
{
    /// <summary>
    /// A survey question definition used for generating survey boxes
    /// </summary>
    [System.Serializable]
    public class SurveyQuestion
    {
        [Tooltip("Description on the survey question box")]
        public string description;
        [Tooltip("Data column this belongs to")]
        public string dataColumn;
    };

    [Tooltip("The survey questions to ask")]
    public SurveyQuestion[] questions = new SurveyQuestion[0];

    /// <summary>
    /// Tracks the responses previously made by the player. Used to show the previous response marker.
    /// </summary>
    private static Dictionary<string, float> previousResponses = new Dictionary<string, float>();

    /// <summary>
    /// Tracks the responses currently being made by the player
    /// </summary>
    private static Dictionary<string, float> currentResponses = new Dictionary<string, float>();

    [Tooltip("The survey box UI to update as we progress through the survey")]
    public SurveyBox surveyBox;

    [Tooltip("Status box UI")]
    public Text questionsRemaining;
    [Tooltip("The continue button")]
    public Button continueButton;
    [Tooltip("The text on the continue button")]
    public Text continueButtonText;

    /// <summary>
    /// Current question in the list of question that we're at
    /// </summary>
    int currentQuestionIndex = 0;

    /// <summary>
    /// Called upon Unity upon first script activation. Resets to the first question
    /// </summary>
    public void OnEnable()
    {
        SetQuestion(0);
    }

    /// <summary>
    /// Applies the given SurveyQuestion to the UI
    /// </summary>
    public void SetQuestion(int questionIndex)
    {
        surveyBox.description = questions[questionIndex].description;
        surveyBox.surveyQuestionIndex = questionIndex;

        if (previousResponses.ContainsKey(questions[questionIndex].dataColumn))
        {
            surveyBox.value = previousResponses[questions[questionIndex].dataColumn];
            surveyBox.previousValue = previousResponses[questions[questionIndex].dataColumn];
        }
        else
        {
            surveyBox.value = 3;
            surveyBox.previousValue = -1;
        }

        if (currentQuestionIndex == questions.Length - 1)
        {
            if (GameManager.singleton.currentRound + 1 < GameManager.singleton.numRounds)
            {
                continueButtonText.text = "Next round!";
            }
            else
            {
                continueButtonText.text = "Finish game!";
            }
        }

        questionsRemaining.text = $"Question {currentQuestionIndex + 1} of {questions.Length}...";

        continueButton.interactable = false;
    }

    /// <summary>
    /// Progresses to the next question
    /// </summary>
    public void OnClickedNextButton()
    {
        // Record this responsee
        float response = currentResponses[questions[currentQuestionIndex].dataColumn];

        GameManager.singleton.data.sessionData[$"{questions[currentQuestionIndex].dataColumn}{GameManager.singleton.activeImpostorConfigurationSymbol}"] = response.ToString("0.00");

        previousResponses[questions[currentQuestionIndex].dataColumn] = response;

        if (currentQuestionIndex < questions.Length - 1)
        {
            // Move to next question
            currentQuestionIndex++;

            SetQuestion(currentQuestionIndex);
        }
        else
        {
            GameManager.singleton.StartNextRound();
        }
    }

    /// <summary>
    /// Called when the slider is clicked
    /// </summary>
    public void OnClickedSlider()
    {
        continueButton.interactable = true;
    }

    /// <summary>
    /// Sets the response value for a given question index
    /// </summary>
    public void SetQuestionValue(int questionIndex, float value)
    {
        currentResponses[questions[questionIndex].dataColumn] = value;
    }
}