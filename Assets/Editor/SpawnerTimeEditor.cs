using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnerTime))]
public class SpawnerTimeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // DrawDefaultInspector();

        SpawnerTime mySpawnerTime = (SpawnerTime) target;
        mySpawnerTime.enemy = (GameObject) EditorGUILayout.ObjectField("Enemy",mySpawnerTime.enemy ,typeof(GameObject), false);
        mySpawnerTime.maxTimer = EditorGUILayout.FloatField("Max Timer", mySpawnerTime.maxTimer);
        mySpawnerTime.timer = EditorGUILayout.Slider("Timer", mySpawnerTime.timer,0,mySpawnerTime.maxTimer);

        mySpawnerTime.maxWaitTimer = EditorGUILayout.FloatField("Max Wait Timer", mySpawnerTime.maxWaitTimer);
        mySpawnerTime.waitTimer = EditorGUILayout.Slider("Wait Timer", mySpawnerTime.waitTimer,0,mySpawnerTime.maxWaitTimer);
        
        if (GUILayout.Button("Build Enemy"))
        {
            mySpawnerTime.Spawn();
        }
    }
}
