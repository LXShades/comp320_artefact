using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(BalloonSpawner))]
public class BalloonSpawnerEditor : Editor
{
    void OnSceneGUI()
    {
        BalloonSpawner spawner = target as BalloonSpawner;
        foreach (BalloonSpawner.SpawnBurst burst in (target as BalloonSpawner).spawnBursts)
        {
            burst.startVelocity = UnityEditor.Handles.DoPositionHandle(spawner.transform.position + burst.startVelocity, Quaternion.identity) - spawner.transform.position;
            burst.endVelocity = UnityEditor.Handles.DoPositionHandle(spawner.transform.position + burst.endVelocity, Quaternion.identity) - spawner.transform.position;
        }
    }
}
#endif