using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// He floats, he pops, he never attacks, he pollutes the environment
/// </summary>
public class BalloonEnemy : MonoBehaviour
{
    // How far up/down the balloon will bob
    public float bobHeight;
    // How rapidly the balloon will bob
    public float bobRate = 0.5f;
    // When the balloon will self-destruct to remove itself from the scene
    public float selfDestructTime = 8.0f;
    // Direction of the balloon
    public Vector3 velocity;
    // Particles to spawn when the balloon pops
    public GameObject popParticles;

    // Spawn point of the balloon
    private Vector3 initialPosition;
    // Time.time at spawn
    private float spawnTime;

    // Components
    private Rigidbody rb;

    void Awake()
    {
        initialPosition = transform.position;
        spawnTime = Time.time;

        rb = GetComponent<Rigidbody>();

        GameManager.singleton.numTotalBalloons++;
    }

    // Update is called once per frame
    void Update()
    {
        float aliveTime = Time.time - spawnTime;

        // Move the balloon along the path
        transform.position = initialPosition + velocity * aliveTime;
        // and bob
        transform.position += Vector3.up * (bobHeight * Mathf.Sin(bobRate * Mathf.PI));

        // Pass the momentum onto the rigidbody so collisions can be detected
        rb.velocity = velocity;
        rb.useGravity = false;

        // Destroy the balloon after a period
        if (aliveTime > selfDestructTime)
        {
            Pop();
        }
    }

    void FixedUpdate()
    {
        rb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Pops the balloon on contact with anything
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<RockProjectile>() != null)
        {
            // The player hit!
            // Add to their score?
            GameManager.singleton.numPoppedBalloons++;
        }

        if (collision.collider.GetComponent<BalloonEnemy>() != null)
        {
            velocity += collision.relativeVelocity;
        }
        else
        {
            // Die
            Pop();
        }
    }

    public void Pop()
    {
        Instantiate(popParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
