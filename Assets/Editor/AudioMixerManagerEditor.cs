using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioMixerManager))]
public class AudioMixerManagerEditor : Editor
{
    // private SerializedProperty mainLevel_background_clip;
    // private SerializedProperty bossBattle_background_clip;
    // private SerializedProperty gameComplete_background_clip;
    // private SerializedProperty levelVictory_background_clip;
    // private SerializedProperty levelSelect_background_clip;
    // private SerializedProperty titleScreen_background_clip;
    //
    // private SerializedProperty bg_Source_1;
    // private SerializedProperty bg_Source_2;
    // private SerializedProperty bg_Source_3;
    //
    // private void OnEnable()
    // {
    //     mainLevel_background_clip = serializedObject.FindProperty("background_MainLevel_Clip");
    //     bossBattle_background_clip = serializedObject.FindProperty("background_BossBattle_Clip");
    //     gameComplete_background_clip = serializedObject.FindProperty("background_GameComplete_Clip");
    //     levelVictory_background_clip = serializedObject.FindProperty("background_LevelVictory_Clip");
    //     levelSelect_background_clip = serializedObject.FindProperty("background_LevelSelect_Clip");
    //     titleScreen_background_clip = serializedObject.FindProperty("background_TitleScreen_Clip");
    //     bg_Source_1 = serializedObject.FindProperty("backgroundSource");
    //     bg_Source_2 = serializedObject.FindProperty("background2Source");
    //     bg_Source_3 = serializedObject.FindProperty("background3Source");
    // }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        // serializedObject.UpdateIfDirtyOrScript();
        //
        // AudioMixerManager myAudioMixer = (AudioMixerManager) target;
        // if (GUILayout.Button("Background"))
        // {
        //     mainLevel_background_clip.
        // }
        //
        // serializedObject.ApplyModifiedProperties();
    }
}
