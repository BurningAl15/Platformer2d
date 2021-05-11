using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ComboManager))]
public class ComboManagerEditor : Editor
{
    [SerializeField] private float thumbnailWidth = 50;
    [SerializeField] private float thumbnailHeight = 50;
    public override void OnInspectorGUI()
    {
        ComboManager myComboManager = (ComboManager) target;
        myComboManager.maxTimer = EditorGUILayout.FloatField("Max Timer", myComboManager.maxTimer);
        // EditorGUILayout.LabelField("TimerActive",myComboManager.isRunning.ToString());
        GUILayout.BeginHorizontal();
        if (myComboManager.isRunning)
        {
            GUILayout.Box(Resources.Load<Texture>("Thumbnails/Thumbnails_1"),
                GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight));
        }
        else
        {
            GUILayout.Box(Resources.Load<Texture>("Thumbnails/Thumbnails_2"),
                GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight));
        }
        
        GUILayout.EndHorizontal();
    }
}
