using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnerTime))]
public class SpawnerTimeEditor : Editor
{
    [SerializeField] private float thumbnailWidth = 50;
    [SerializeField] private float thumbnailHeight = 50;

    public override void OnInspectorGUI()
    {
        // DrawDefaultInspector();

        SpawnerTime mySpawnerTime = (SpawnerTime) target;
        mySpawnerTime.enemy = (GameObject) EditorGUILayout.ObjectField("Enemy",mySpawnerTime.enemy ,typeof(GameObject), false);
        mySpawnerTime.maxTimer = EditorGUILayout.FloatField("Max Timer", mySpawnerTime.maxTimer);
        mySpawnerTime.timer = EditorGUILayout.Slider("Timer", mySpawnerTime.timer,0,mySpawnerTime.maxTimer);

        mySpawnerTime.maxWaitTimer = EditorGUILayout.FloatField("Max Wait Timer", mySpawnerTime.maxWaitTimer);
        mySpawnerTime.waitTimer = EditorGUILayout.Slider("Wait Timer", mySpawnerTime.waitTimer,0,mySpawnerTime.maxWaitTimer);

        mySpawnerTime.direction = EditorGUILayout.IntField("Direction", mySpawnerTime.direction);
        
        GUILayout.BeginHorizontal();
        if (mySpawnerTime.runSpawning)
        {
            GUILayout.Button(Resources.Load<Texture>("Thumbnails/Thumbnails_1"),
                GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight));
        }
        else
        {
            GUILayout.Button(Resources.Load<Texture>("Thumbnails/Thumbnails_2"),
                GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight));
        }
        
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Build Enemy"))
        {
            mySpawnerTime.Spawn();
        }
    }
}
