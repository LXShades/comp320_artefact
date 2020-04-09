using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Once Activataed, spawns balloons in bursts at configurable times
/// </summary>
public class BalloonSpawner : MonoBehaviour
{
    /// <summary>
    /// Spawns some balloons at the given time
    /// </summary>
    [System.Serializable]
    public class SpawnBurst
    {
        [Tooltip("Time relative to the beginning of the cycle that the spawn burst will occur")]
        public float time = 0;

        [Tooltip("Number to spawn")]
        public int numToSpawn = 1;

        [Tooltip("Wait time between each spawn")]
        public float spawnTimeGap;

        [Tooltip("Initial velocity to spawn the balloons at")]
        public Vector3 startVelocity = Vector3.one;

        [Tooltip("Ending velocity (velocity of the final balloon) to spawn the balloons at")]
        public Vector3 endVelocity = Vector3.one;
    }

    [Tooltip("The balloon prefab to spawn")]
    public GameObject prefabToSpawn;

    [Tooltip("List of spawn bursts in a cycle")]
    public List<SpawnBurst> spawnBursts = new List<SpawnBurst>();

    [Tooltip("Whether to activate at level start")]
    public bool autoActivate = false;

    /// <summary>
    /// Game time that we were activated at
    /// </summary>
    float activationTime = 0;

    /// <summary>
    /// Whether the spawner is active and spawning balloons
    /// </summary>
    bool hasActivated = false;

    /// <summary>
    /// Called by Unity on creation. Auto-activates if desired.
    /// </summary>
    void Start()
    {
        if (autoActivate)
        {
            Activate();
        }
    }

    /// <summary>
    /// Called by Unity each frame. If activated, spawns balloons at the appropriate times
    /// </summary>
    void Update()
    {
        if (hasActivated)
        {
            float currentTime = Time.time - activationTime;
            float lastTime = currentTime - Time.deltaTime;

            // Spawn balloons
            foreach (SpawnBurst burst in spawnBursts)
            {
                if (currentTime >= burst.time && lastTime < burst.time)
                {
                    // This burst just started
                    SpawnBalloon(transform.position, burst.startVelocity);
                }
                else if (currentTime >= burst.time && lastTime - burst.time <= (burst.numToSpawn - 1) * burst.spawnTimeGap)
                {
                    // Check if we're crossing one of the bursts in a multiple-burst burst. burst.
                    if (((currentTime - burst.time) % burst.spawnTimeGap) < ((lastTime - burst.time) % burst.spawnTimeGap))
                    {
                        float lerpFactor = burst.numToSpawn >= 2 ? (currentTime - burst.time) / (burst.spawnTimeGap * (burst.numToSpawn - 1)) : 0.5f;
                        SpawnBalloon(transform.position, Vector3.Lerp(burst.startVelocity, burst.endVelocity, lerpFactor));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Begins the enemy spawn cycle
    /// </summary>
    public void Activate()
    {
        if (!hasActivated)
        {
            activationTime = Time.time;

            hasActivated = true;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Visualises the spawn bursts with lines
    /// </summary>
    void OnDrawGizmos()
    {
        foreach (SpawnBurst burst in spawnBursts)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + burst.startVelocity);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + burst.endVelocity);
        }
    }
#endif

    /// <summary>
    /// Spawns a new balloon at the given position and velocity
    /// </summary>
    void SpawnBalloon(Vector3 position, Vector3 velocity)
    {
        BalloonEnemy balloon = Instantiate(prefabToSpawn, position, Quaternion.identity).GetComponent<BalloonEnemy>();

        balloon.velocity = velocity;
    }
}
