using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnerSwitch))]
public class SpawnerSwitchEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpawnerSwitch mySpawnerSwitch = (SpawnerSwitch) target;
        if (GUILayout.Button("Build Enemy"))
        {
            mySpawnerSwitch.Spawn();
        }
    }
}
