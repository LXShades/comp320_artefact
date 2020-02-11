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
            EditorGUI.BeginChangeCheck();
            Vector3 startVelocity = UnityEditor.Handles.DoPositionHandle(spawner.transform.position + burst.startVelocity, Quaternion.identity) - spawner.transform.position;
            Vector3 endVelocity = UnityEditor.Handles.DoPositionHandle(spawner.transform.position + burst.endVelocity, Quaternion.identity) - spawner.transform.position;

            UnityEditor.Handles.color = new Color(1, 1, 1, 0.25f);
            UnityEditor.Handles.DrawSphere(0, spawner.transform.position + burst.startVelocity, Quaternion.identity, 0.5f);
            UnityEditor.Handles.DrawSphere(0, spawner.transform.position + burst.endVelocity, Quaternion.identity, 0.5f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed spawner velocity(s)");

                burst.startVelocity = startVelocity;
                burst.endVelocity = endVelocity;
            }
        }
    }
}
#endif