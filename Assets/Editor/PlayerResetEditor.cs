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
    
    private Checkpoint[] _checkpoints;
    private Transform currentCheckpointPos;
    private int index;
    private int checkpointsInScene = 0;

    private string customString = "String Here";
    private bool toInitialPosition;
    private bool toCheckpointPosition;
    private bool optionalSettings = true;
    private float jumpMod = 1.0f;

    private Vector3 initialPosition;

    private bool initialized = false;
    
    [MenuItem("CustomEditorTools/Reset PlayerPosition")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PlayerResetEditor));
    }

    void InitializeData_InitialPosition()
    {
        toCheckpointPosition = false;
        if (player == null)
        {
            tempGameobject = GameObject.Find("Player");
            player = tempGameobject.GetComponent<PlayerController2d>();
        }

        tempGameobject = GameObject.Find("InitialPoint");
        initialPosition_T = tempGameobject.transform;
    }

    void InitializeData_Checkpoint()
    {
        toInitialPosition = false;
        if (player == null)
        {
            tempGameobject = GameObject.Find("Player");
            player = tempGameobject.GetComponent<PlayerController2d>();
        }
        
        _checkpoints=FindObjectsOfType<Checkpoint>();
        checkpointsInScene = _checkpoints.Length;
        index = Mathf.Clamp(index, 0, _checkpoints.Length-1);
        currentCheckpointPos = _checkpoints[index].transform;
    }

    private void OnGUI()
    {
        GUILayout.Label("Set Player Position",EditorStyles.boldLabel);

        toInitialPosition = EditorGUILayout.BeginToggleGroup("Reset to Initial Position the Player?", toInitialPosition);

        if (toInitialPosition)
        {
            initialized = false;
            if (!initialized)
            {
                InitializeData_InitialPosition();
                initialized = true;
            }
            
            optionalSettings = EditorGUILayout.Toggle("Use Transform?", optionalSettings);

            if (optionalSettings)
                initialPosition_T = (Transform) EditorGUILayout.ObjectField("Initial Position", initialPosition_T, typeof(Transform), true);
            else
                initialPosition_V3 = EditorGUILayout.Vector2Field("Initial Position", initialPosition_V3);
        
            player = (PlayerController2d) EditorGUILayout.ObjectField("Player", player, typeof(PlayerController2d), true);
        
            if (GUILayout.Button("Reset To Initial Position"))
            {
                initialPosition = optionalSettings ? initialPosition_T.position : initialPosition_V3;
                player.transform.position = initialPosition;
            }
            
        }
        EditorGUILayout.EndToggleGroup();
        
        toCheckpointPosition = EditorGUILayout.BeginToggleGroup("Set to Checkpoint Position the Player?", toCheckpointPosition);
        
        if(toCheckpointPosition)
        {
            initialized = false;
            if (!initialized)
            {
                InitializeData_Checkpoint();
                initialized = true;
            }
            
            EditorGUILayout.LabelField("Number of Checkpoints in scene: ", checkpointsInScene.ToString());
            index = EditorGUILayout.IntField("Checkbox Index: ", index);
        
            if (GUILayout.Button("Set me to the Index Checkbox"))
            {
                player.transform.position = currentCheckpointPos.transform.position + Vector3.up * 0.49115f;
            }
        }
        EditorGUILayout.EndToggleGroup();
    }
    
    void DrawOnGUISprite(Sprite aSprite)
    {
        Rect c = aSprite.rect;
        float spriteW = c.width;
        float spriteH = c.height;
        Rect rect = GUILayoutUtility.GetRect(spriteW, spriteH);
        if (Event.current.type == EventType.Repaint)
        {
            var tex = aSprite.texture;
            c.xMin /= tex.width;
            c.xMax /= tex.width;
            c.yMin /= tex.height;
            c.yMax /= tex.height;
            GUI.DrawTextureWithTexCoords(rect, tex, c);
        }
    }
    
    Texture2D GenerateTextureFromSprite(Sprite aSprite)
    {
        var rect = aSprite.rect;
        var tex = new Texture2D((int)rect.width, (int)rect.height);
        var data = aSprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        tex.SetPixels(data);
        tex.Apply(true);
        return tex;
    }
}
