using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A volume that triggers associated spawners to spawn balloons
/// </summary>
public class SpawnZone : MonoBehaviour
{
    [Tooltip("Balloon spawners to activate when the player enters this zone")]
    public BalloonSpawner[] associatedSpawners = new BalloonSpawner[0];

    /// <summary>
    /// Called upon creation by Unity. Hides the trigger volume.
    /// </summary>
    void Start()
    {
        // Don't show the volume in-game
        GetComponent<Renderer>().enabled = false;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Shows lines pointing to associated spawners
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (BalloonSpawner spawner in associatedSpawners)
        {
            Gizmos.DrawLine(transform.position + Vector3.up * 2.5f, spawner.transform.position);
        }
    }
#endif

    /// <summary>
    /// Called by Unity upon entering the trigger. Activates the associated ballon spawners
    /// </summary>
    void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponentInParent<Player>())
        {
            foreach (BalloonSpawner spawner in associatedSpawners)
            {
                spawner.Activate();
            }
        }
    }
}
