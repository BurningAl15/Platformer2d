using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CountGemsInLevel : EditorWindow
{
    private int gems = 0;
    private List<Pickup> gemsFound = new List<Pickup>();
    
    [SerializeField] private float thumbnailWidth = 50;
    [SerializeField] private float thumbnailHeight = 50;

    private GUIStyle guiStyle = new GUIStyle(); //create a new variable
    
    [MenuItem("CustomEditorTools/Get Gems in Level")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CountGemsInLevel));
    }

    void InitializeData()
    {
        gems = 0;
        Pickup[] tempPicks = FindObjectsOfType<Pickup>();

        for (int i = 0; i < tempPicks.Length; i++)
        {
            if (tempPicks[i].IsGem())
            {
                gemsFound.Add(tempPicks[i]);
                gems++;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Get The Number of Gems In Level", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        guiStyle.fontSize = 20; //change the font size
        guiStyle.alignment = TextAnchor.MiddleLeft;
        // guiStyle.
        EditorGUILayout.BeginHorizontal();
        GUILayout.Box(Resources.Load<Texture>("Thumbnails/Gems"),
            GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight));
        EditorGUILayout.LabelField("", gems.ToString(),guiStyle);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        // EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        for (int i = 0; i < gemsFound.Count; i++)
        {
            // GUILayout.Box( gemsFound[i].PickupTransform.position.ToString(),GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight));
            GUILayout.Label($"Gem {i+1} - {gemsFound[i].PickupTransform.position.ToString()}");
        }
        // EditorGUILayout.EndHorizontal();


        if (GUILayout.Button("Counting Gems"))
        {
            InitializeData();
        }
    }
}
