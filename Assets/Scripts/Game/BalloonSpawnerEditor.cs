using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Balloon spawner editor that previews balloon spawns in real-time
/// </summary>
[CustomEditor(typeof(BalloonSpawner))]
public class BalloonSpawnerEditor : Editor
{
    /// <summary>
    /// Time.realTimeSinceStartup at the last frame. Used to calculate deltaTime.
    /// </summary>
    private float lastTime;

    /// <summary>
    /// Time since this object was activated
    /// </summary>
    private float cycleTime = 0;

    /// <summary>
    /// The last rotation of the object. If this differs from the current rotation, the simulation is adjusted
    /// </summary>
    private Quaternion lastRotation;

    /// <summary>
    /// Forces the editor to update the object (for simulation purposes)
    /// </summary>
    public void OnEnable()
    {
        cycleTime = 0;
        lastTime = Time.realtimeSinceStartup;

        lastRotation = (target as BalloonSpawner).transform.rotation;

        // Force continual update so we can visualise balloon paths
        EditorApplication.update += Update;
    }

    /// <summary>
    /// Detaches from editor updates
    /// </summary>
    public void OnDisable() { EditorApplication.update -= Update; }

    /// <summary>
    /// Called during an editor viewport update. Continues the balloon spawn simulation
    /// </summary>
    void Update()
    {
        float deltaTime = Time.realtimeSinceStartup - lastTime;
        BalloonSpawner spawner = target as BalloonSpawner;

        cycleTime += deltaTime;
        lastTime = Time.realtimeSinceStartup;

        if (spawner.transform.rotation != lastRotation)
        {
            Quaternion delta = spawner.transform.rotation * Quaternion.Inverse(lastRotation);

            foreach (BalloonSpawner.SpawnBurst burst in spawner.spawnBursts)
            {
                burst.startVelocity = delta * burst.startVelocity;
                burst.endVelocity = delta * burst.endVelocity;
            }

            lastRotation = spawner.transform.rotation;
        }
    }

    /// <summary>
    /// Renders spawn start/end handles and simulated balloons
    /// </summary>
    void OnSceneGUI()
    {
        // Draw spawn direction handles
        BalloonSpawner spawner = target as BalloonSpawner;
        foreach (BalloonSpawner.SpawnBurst burst in spawner.spawnBursts)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 startVelocity = UnityEditor.Handles.DoPositionHandle(spawner.transform.position + burst.startVelocity, Quaternion.identity) - spawner.transform.position;
            Vector3 endVelocity = UnityEditor.Handles.DoPositionHandle(spawner.transform.position + burst.endVelocity, Quaternion.identity) - spawner.transform.position;

            Handles.color = new Color(1, 1, 1, 0.25f);
            Handles.DrawSphere(0, spawner.transform.position + burst.startVelocity, Quaternion.identity, 0.5f);
            Handles.DrawSphere(0, spawner.transform.position + burst.endVelocity, Quaternion.identity, 0.5f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed spawner velocity(s)");

                burst.startVelocity = startVelocity;
                burst.endVelocity = endVelocity;

                cycleTime = 0;
            }
        }

        // Draw balloon motion visualisers
        float balloonLifetime = 5.0f;
        foreach (BalloonSpawner.SpawnBurst burst in spawner.spawnBursts)
        {
            float burstRelativeTime = cycleTime - burst.time;

            if (burstRelativeTime >= 0 && burstRelativeTime < cycleTime + burst.numToSpawn * burst.spawnTimeGap + balloonLifetime)
            {
                for (int i = 0; i < burst.numToSpawn; i++)
                {
                    float balloonRelativeTime = burstRelativeTime - i * burst.spawnTimeGap;
                    if (balloonRelativeTime >= 0 && balloonRelativeTime <= balloonLifetime)
                    {
                        float lerpFactor = burst.numToSpawn > 1 ? (float)i / (burst.numToSpawn - 1) : 0.5f;
                        Vector3 velocity = Vector3.Lerp(burst.startVelocity, burst.endVelocity, lerpFactor);
                        Matrix4x4 balloonMatrix = Matrix4x4.TRS(spawner.transform.position + velocity * balloonRelativeTime, Quaternion.identity, spawner.prefabToSpawn.transform.localScale);

                        spawner.prefabToSpawn.GetComponentInChildren<MeshRenderer>().sharedMaterial.SetPass(4);
                        Graphics.DrawMeshNow(spawner.prefabToSpawn.GetComponent<MeshFilter>().sharedMesh, balloonMatrix, 0);
                    }
                }
            }

            HandleUtility.Repaint();
        }
    }
}
#endif