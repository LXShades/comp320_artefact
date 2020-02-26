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

    // Speed damping when colliding
    public float collisionDamp = 0.5f;

    // Speed in degrees/s that the rock will spin
    public float spinSpeed = 700.0f;

    // Scale multiplier over time after the rock is spawned
    public AnimationCurve scaleCurve;

    // Rigidbody component
    private Rigidbody rb;

    // Scale this object was spawned with
    private Vector3 initialScale;

    // Time.time this object was spawned at
    private float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialScale = transform.localScale;

        spawnTime = Time.time;
    }

    void Update()
    {
        transform.localScale = initialScale * scaleCurve.Evaluate(Time.time - spawnTime);
    }

    /// <summary>
    /// Sets trajectory to hit the given target position
    /// </summary>
    /// <param name="targetPosition">Target to travel towards</param>
    public void Shoot(Vector3 targetPosition)
    {
        rb.AddForce((targetPosition - transform.position).normalized * speed, ForceMode.VelocityChange);
        rb.AddTorque(Random.onUnitSphere * spinSpeed, ForceMode.Impulse);
    }

    /// <summary>
    /// Stops the rock from bouncing forever
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        rb.useGravity = true;
        rb.velocity *= collisionDamp;
    }
}
