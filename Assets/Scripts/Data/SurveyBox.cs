using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Data reporter for survey box UI components
/// </summary>
public class SurveyBox : MonoBehaviour
{
    // The UI element containg the text description for the question
    public Text descriptionText;

    // Slider tracking the user's response
    public Slider slider;

    // Marker showing the user's previous response
    public Canvas previousMarker;

    // Current survey question index of parent SurveySequence
    [HideInInspector]
    public int surveyQuestionIndex = 0;

    // Parent survey sequence
    private SurveySequence parentSequence;

    // Sets or retrieves the user's current response value
    public float value
    {
        set
        {
            slider.value = value;
        }
        get
        {
            return slider.value;
        }
    }

    // Sets or receives the marker of the previous value's position, or -1 for none
    public float previousValue
    {
        get
        {
            return _previousValue;
        }
        set
        {
            _previousValue = value;

            RectTransform rect = previousMarker.GetComponent<RectTransform>();
            RectTransform parentRect = previousMarker.transform.parent.GetComponent<RectTransform>();

            rect.anchoredPosition = new Vector3(rect.anchoredPosition.x, (slider.maxValue - value) / (slider.maxValue - slider.minValue) * -parentRect.sizeDelta.y);

            previousMarker.gameObject.SetActive(value > 0);
        }
    }
    private float _previousValue;


    // Retrieves or sets the description directly
    public string description
    {
        get
        {
            return descriptionText.text;
        }
        set
        {
            descriptionText.text = value;
        }
    }

    /// <summary>
    /// Writes an initial value to the data file
    /// </summary>
    void Start()
    {
        parentSequence = GetComponentInParent<SurveySequence>();

        OnChange();
    }

    /// <summary>
    /// Writes value to the data file
    /// </summary>
    public void OnChange()
    {
        parentSequence.SetQuestionValue(surveyQuestionIndex, slider.value);
    }
}
