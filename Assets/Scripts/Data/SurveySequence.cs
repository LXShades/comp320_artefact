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
        surveyBox.entryName = questions[questionIndex].dataColumn;
        surveyBox.slider.value = 3;

        if (currentQuestionIndex == questions.Length - 1)
        {
            questionsRemaining.text = "Last question!";
            continueButtonText.text = "Next round!";
        }
        else
        {
            questionsRemaining.text = $"{questions.Length - currentQuestionIndex - 1} more to go...";
        }

        continueButton.interactable = false;
    }

    /// <summary>
    /// Goes to the next question
    /// </summary>
    public void OnClickedNextButton()
    {
        if (currentQuestionIndex < questions.Length - 1)
        {
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
}