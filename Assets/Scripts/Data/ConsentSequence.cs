using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runs the consent form UI
/// </summary>
public class ConsentSequence : MonoBehaviour
{
    // Sequence of forms to go through with each button press
    public GameObject[] forms;

    int currentFormIndex = 0;

    void Start()
    {
        // Turn off all forms before activating the current one
        foreach (GameObject form in forms)
        {
            form.SetActive(false);
        }

        OpenForm(currentFormIndex);
    }

    /// <summary>
    /// Opens the form with the given index
    /// </summary>
    void OpenForm(int index)
    {
        forms[currentFormIndex].SetActive(false);
        forms[index].SetActive(true);

        currentFormIndex = index;
    }

    /// <summary>
    /// Moves to the next part
    /// </summary>
    public void OnConsentButtonClicked()
    {
        if (currentFormIndex < forms.Length - 1)
        {
            OpenForm(currentFormIndex + 1);
        }
        else
        {
            GameManager.singleton.StartNextRound();
        }
    }
}
