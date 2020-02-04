using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonSpawner : MonoBehaviour
{
    /// <summary>
    /// Spawns some balloons at the given time
    /// </summary>
    [System.Serializable]
    public class SpawnBurst
    {
        // Time relative to the beginning of the cycle that the spawn burst will occur
        public float time = 0;

        // Number to spawn
        public int numToSpawn = 1;

        // Wait time between each spawn
        public float spawnTimeGap;

        // Initial velocity to spawn the balloons at
        public Vector3 startVelocity = Vector3.one;

        // Ending velocity (velocity of the final balloon) to spawn the balloons at
        public Vector3 endVelocity = Vector3.one;
    }

    // The balloon prefab to spawn
    public GameObject prefabToSpawn;

    // List of spawn bursts in a cycle
    public List<SpawnBurst> spawnBursts = new List<SpawnBurst>();

    // Whether to activate at level start
    public bool autoActivate = false;

    // Game time that the beginning of the cycle started at
    float activationTime = 0;

    // Whether the spawner is active and spawning balloons
    bool hasActivated = false;

    // Start is called before the first frame update
    void Start()
    {
        if (autoActivate)
        {
            Activate();
        }
    }

    // Update is called once per frame
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
                        SpawnBalloon(transform.position, Vector3.Lerp(burst.startVelocity, burst.endVelocity, (currentTime - burst.time) / (burst.spawnTimeGap * burst.numToSpawn)));
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

    void SpawnBalloon(Vector3 position, Vector3 velocity)
    {
        BalloonEnemy balloon = Instantiate(prefabToSpawn, position, Quaternion.identity).GetComponent<BalloonEnemy>();

        balloon.velocity = velocity;
    }
}
