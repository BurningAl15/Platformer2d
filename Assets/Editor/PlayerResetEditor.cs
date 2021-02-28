using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerResetEditor : EditorWindow
{
    private Vector3 initialPosition_V3;
    private Transform initialPosition_T;

    private PlayerController2d player;

    private GameObject tempGameobject;
    
    private string customString = "String Here";
    private bool groupEnabled;
    private bool optionalSettings = true;
    private float jumpMod = 1.0f;

    private Vector3 initialPosition;
    
    [MenuItem("CustomEditorTools/Reset PlayerPosition")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PlayerResetEditor));
    }

    void InitializeData()
    {
        tempGameobject = GameObject.Find("Player");
        player = tempGameobject.GetComponent<PlayerController2d>();
        
        tempGameobject = GameObject.Find("InitialPoint");
        initialPosition_T = tempGameobject.transform;
    }

    private void OnGUI()
    {
        GUILayout.Label("Initial Player Position",EditorStyles.boldLabel);
        // customString = EditorGUILayout.TextField("Text Field", customString);

        groupEnabled = EditorGUILayout.BeginToggleGroup("Group", groupEnabled);
        optionalSettings = EditorGUILayout.Toggle("Use Vector?", optionalSettings);

        if(groupEnabled)
            InitializeData();
        
        if (optionalSettings)
            initialPosition_V3 = EditorGUILayout.Vector2Field("Initial Position", initialPosition_V3);
        else
            initialPosition_T = (Transform) EditorGUILayout.ObjectField("Label:", initialPosition_T, typeof(Transform), true);
        
        player = (PlayerController2d) EditorGUILayout.ObjectField("Player", player, typeof(PlayerController2d), true);
        
        EditorGUILayout.EndToggleGroup();
        
        if (GUILayout.Button("Reset To Initial Position"))
        {
            initialPosition = optionalSettings ? initialPosition_V3 : initialPosition_T.position;
            player.transform.position = initialPosition;
        }
    }
}
