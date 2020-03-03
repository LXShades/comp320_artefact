﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Opens a door when hit
/// </summary>
public class DoorSwitch : MonoBehaviour
{
    public Door doorToOpen;

    /// <summary>
    /// Opens the door if hit by the rock
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponentInParent<RockProjectile>())
        {
            doorToOpen?.Open();
        }
    }
}