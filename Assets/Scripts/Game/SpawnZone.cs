using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    public BalloonSpawner[] associatedSpawners = new BalloonSpawner[0];

    // Start is called before the first frame update
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

    void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponentInParent<Player>())
        {
            Debug.Log("Aight im activate spawns...");

            foreach (BalloonSpawner spawner in associatedSpawners)
            {
                spawner.Activate();
            }
        }
        else
        {
            Debug.Log("Who df is this");
        }
    }
}
