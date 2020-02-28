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
        // Description on the survey question box
        public string description;
        // Data column this belongs to
        public string dataColumn;
    };

    // The list of questions 
    public SurveyQuestion[] questions = new SurveyQuestion[0];

    // Tracks the responses previously made by the player
    private static Dictionary<string, float> previousResponses = new Dictionary<string, float>();
    private static Dictionary<string, float> currentResponses = new Dictionary<string, float>();

    // The survey box to update as we progress through the survey
    public SurveyBox surveyBox;

    // Status box
    public Text questionsRemaining;
    // Continue button
    public Button continueButton;
    // Text on the continue button
    public Text continueButtonText;

    // Current question in the list of question that we're at
    int currentQuestionIndex = 0;

    public void Start()
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
            Debug.Log("Key found");
        }
        else
        {
            surveyBox.value = 3;
            surveyBox.previousValue = -1;
            Debug.Log("Key not found");
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

        questionsRemaining.text = $"{currentQuestionIndex + 1}/{questions.Length}";

        continueButton.interactable = false;
    }

    /// <summary>
    /// Goes to the next question
    /// </summary>
    public void OnClickedNextButton()
    {
        // Record this responsee
        float response = currentResponses[questions[currentQuestionIndex].dataColumn];

        GameManager.singleton.data.sessionData[$"{questions[currentQuestionIndex].dataColumn}{GameManager.singleton.impostorConfigurationName}"] = response.ToString("0.00");

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