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

    // Name of the data column that this survey box applies to
    public string entryName;

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
        OnChange();
    }

    /// <summary>
    /// Writes value to the data file
    /// </summary>
    public void OnChange()
    {
        GameManager.singleton.data.sessionData[$"{entryName}{GameManager.singleton.impostorConfigurationName}"] = ((int)slider.value).ToString();
    }
}
