using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rock projectile fired by the player to hit balloons with
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RockProjectile : MonoBehaviour
{
    // Speed in m/s that the rock should travel
    public float speed = 50.0f;

    // Rigidbody component
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Sets trajectory to hit the given target position
    /// </summary>
    /// <param name="targetPosition">Target to travel towards</param>
    public void Shoot(Vector3 targetPosition)
    {
        rb.velocity = (targetPosition - transform.position).normalized * speed;
    }
}
