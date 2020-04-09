using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Data reporter for survey box UI components
/// </summary>
public class SurveyBox : MonoBehaviour
{
    [Tooltip("The UI element containg the text description for the question")]
    public Text descriptionText;

    [Tooltip("Slider tracking the user's response")]
    public Slider slider;

    [Tooltip("Marker showing the user's previous response")]
    public Canvas previousMarker;

    /// <summary>
    /// Current survey question index of parent SurveySequence
    /// </summary>
    [HideInInspector]
    public int surveyQuestionIndex = 0;

    /// <summary>
    /// The survey sequence owning this survey box
    /// </summary>
    private SurveySequence parentSequence;

    /// <summary>
    /// Sets or retrieves the user's current response value from the slider
    /// </summary>
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

    /// <summary>
    /// Sets or receives the marker of the previous value's position, or -1 if none/NA/first time asked
    /// </summary>
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


    /// <summary>
    /// Retrieves or sets the question description directly
    /// </summary>
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
    /// Keep track of the parent component for setting question values
    /// </summary>
    private void OnEnable()
    {
        parentSequence = GetComponentInParent<SurveySequence>();
    }

    /// <summary>
    /// Records the initial value
    /// </summary>
    void Start()
    {
        OnChange();
    }

    /// <summary>
    /// Writes the response value to the data file
    /// </summary>
    public void OnChange()
    {
        if (parentSequence)
        {
            parentSequence.SetQuestionValue(surveyQuestionIndex, slider.value);
        }
    }
}
