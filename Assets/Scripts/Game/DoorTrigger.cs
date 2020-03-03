using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Opens a door, simple as
/// </summary>
public class DoorTrigger : MonoBehaviour
{
    public Door doorToOpen;

    /// <summary>
    /// Opens the door when the player enters
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorToOpen.Open();
            Debug.Log("Open sesame");
        }
    }
}
