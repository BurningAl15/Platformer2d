using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Automatización de builds para Android e iOS.
/// Permite generar builds desde el Editor de Unity (menú superior) o desde la terminal CLI en CI/CD.
/// 
/// COMANDOS CLI:
/// /Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath . -executeMethod BuildAutomation.BuildAndroidAPK
/// /Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath . -executeMethod BuildAutomation.BuildAndroidAAB
/// /Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath . -executeMethod BuildAutomation.BuildiOS
/// </summary>
public static class BuildAutomation
{
    private const string APP_IDENTIFIER = "com.fatcatgamestudio.foxstale";
    private const string PRODUCT_NAME = "Fox's Tale";
    private const string COMPANY_NAME = "Fatcat Game Studio";

    private static readonly string[] RequiredScenes = new string[]
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/LevelSelection.unity",
        "Assets/Scenes/Levels/Level_0.unity",
        "Assets/Scenes/Levels/Level_1.unity",
        "Assets/Scenes/Levels/Level_2.unity",
        "Assets/Scenes/Levels/Level_3.unity",
        "Assets/Scenes/Levels/Level_4.unity",
        "Assets/Scenes/Levels/Level_5.unity",
        "Assets/Scenes/Levels/Level_6.unity",
        "Assets/Scenes/Levels/Level_7.unity",
        "Assets/Scenes/Levels/Level_8.unity",
        "Assets/Scenes/Levels/Level_9.unity",
        "Assets/Scenes/Levels/Level_10.unity"
    };

    [MenuItem("CustomEditorTools/Builds/Build Android APK (Testing)")]
    public static void BuildAndroidAPK()
    {
        PerformAndroidBuild(isAppBundle: false);
    }

    [MenuItem("CustomEditorTools/Builds/Build Android AAB (Google Play)")]
    public static void BuildAndroidAAB()
    {
        PerformAndroidBuild(isAppBundle: true);
    }

    [MenuItem("CustomEditorTools/Builds/Build iOS (Xcode Project)")]
    public static void BuildiOS()
    {
        PerformiOSBuild();
    }

    private static void PerformAndroidBuild(bool isAppBundle)
    {
        Debug.Log($"[BuildAutomation] Iniciando Build Android {(isAppBundle ? "AAB" : "APK")}...");

        ConfigureGeneralSettings();
        ConfigureAndroidSettings(isAppBundle);

        string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Android");
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        string extension = isAppBundle ? ".aab" : ".apk";
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
        string outputPath = Path.Combine(outputDir, $"FoxsTale_{timestamp}{extension}");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetScenesForBuild(),
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        ProcessBuildResult(report, outputPath);
    }

    private static void PerformiOSBuild()
    {
        Debug.Log("[BuildAutomation] Iniciando Build iOS (Xcode)...");

        ConfigureGeneralSettings();
        ConfigureiOSSettings();

        string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "iOS", "XcodeProject");
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetScenesForBuild(),
            locationPathName = outputDir,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        ProcessBuildResult(report, outputDir);
    }

    private static void ConfigureGeneralSettings()
    {
        PlayerSettings.companyName = COMPANY_NAME;
        PlayerSettings.productName = PRODUCT_NAME;
    }

    private static void ConfigureAndroidSettings(bool isAppBundle)
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, APP_IDENTIFIER);
        EditorUserBuildSettings.buildAppBundle = isAppBundle;

        // Target API Level >= 34 (Requerido por Google Play)
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

        // Arquitecturas 64-bit y 32-bit (ARM64 requerido por Google Play)
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

        // Permisos de Internet requeridos para Ads y Unity Gaming Services
        PlayerSettings.Android.forceInternetPermission = true;
    }

    private static void ConfigureiOSSettings()
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, APP_IDENTIFIER);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
    }

    private static string[] GetScenesForBuild()
    {
        List<string> scenes = new List<string>();

        // Usar escenas requeridas predeterminadas si los archivos existen en disco
        foreach (string scenePath in RequiredScenes)
        {
            if (File.Exists(scenePath))
            {
                scenes.Add(scenePath);
            }
        }

        // Si por alguna razón ninguna existe, fallback a EditorBuildSettings
        if (scenes.Count == 0)
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }
        }

        return scenes.ToArray();
    }

    private static void ProcessBuildResult(BuildReport report, string outputPath)
    {
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildAutomation] ✅ Build EXITOSO en {summary.totalTime.TotalSeconds:F1}s.");
            Debug.Log($"[BuildAutomation] Archivo generado: {outputPath} ({summary.totalSize / (1024 * 1024):F2} MB)");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"[BuildAutomation] ❌ Build FALLÓ con {summary.totalErrors} errores.");
        }
    }
}
