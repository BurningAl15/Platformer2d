using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

    private static readonly string[] RequiredScenes = new string[]
    {
        "Assets/FatCatStudio/Splash/FatCatSplash.unity",
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

    private static string GetBuildSetting(string name)
    {
        string value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrEmpty(value))
            return value;

        // Editor menu builds can read only this project's whitelisted values
        // from the user's mode-600 config; never source/evaluate the whole file.
        if (name != "FOXSTALE_KEYSTORE_PATH" &&
            name != "FOXSTALE_KEYSTORE_PASSWORD" &&
            name != "FOXSTALE_KEYALIAS_NAME" &&
            name != "FOXSTALE_KEYALIAS_PASSWORD")
            return null;

        string home = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(home))
            home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string configPath = Path.Combine(home, ".fatcat-build", "secrets.env");
        if (!File.Exists(configPath))
            return null;

        foreach (string line in File.ReadAllLines(configPath))
        {
            int separator = line.IndexOf('=');
            if (separator < 0 || !string.Equals(line.Substring(0, separator).Trim(), name, StringComparison.Ordinal))
                continue;

            string configuredValue = line.Substring(separator + 1).Trim();
            if (configuredValue.Length >= 2 &&
                ((configuredValue[0] == '"' && configuredValue[configuredValue.Length - 1] == '"') ||
                 (configuredValue[0] == '\'' && configuredValue[configuredValue.Length - 1] == '\'')))
                configuredValue = configuredValue.Substring(1, configuredValue.Length - 2);
            return configuredValue;
        }

        return null;
    }

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
        int previousVersionCode = PlayerSettings.Android.bundleVersionCode;
        bool previousUseCustomKeystore = PlayerSettings.Android.useCustomKeystore;
        string previousKeystoreName = PlayerSettings.Android.keystoreName;
        string previousAliasName = PlayerSettings.Android.keyaliasName;
        string previousKeystorePassword = PlayerSettings.Android.keystorePass;
        string previousAliasPassword = PlayerSettings.Android.keyaliasPass;
        bool previousAppBundle = EditorUserBuildSettings.buildAppBundle;

        try
        {
            ConfigureGeneralSettings();
            ConfigureAndroidSettings(isAppBundle);

            // APKs are local device-test artifacts. Use Unity's debug signing
            // instead of requiring the private release keystore; AAB releases
            // keep the configured production signing path above.
            if (!isAppBundle)
                PlayerSettings.Android.useCustomKeystore = false;

            string outputDir = Path.Combine(ProjectRoot, "Builds", "Android");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string extension = isAppBundle ? ".aab" : ".apk";
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string outputPath = Environment.GetEnvironmentVariable("FOXSTALE_ANDROID_OUTPUT_PATH");
            if (string.IsNullOrEmpty(outputPath))
                outputPath = Path.Combine(outputDir, $"FoxsTale_{timestamp}{extension}");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)));

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
        finally
        {
            PlayerSettings.Android.bundleVersionCode = previousVersionCode;
            PlayerSettings.Android.useCustomKeystore = previousUseCustomKeystore;
            PlayerSettings.Android.keystoreName = previousKeystoreName;
            PlayerSettings.Android.keyaliasName = previousAliasName;
            PlayerSettings.Android.keystorePass = previousKeystorePassword;
            PlayerSettings.Android.keyaliasPass = previousAliasPassword;
            EditorUserBuildSettings.buildAppBundle = previousAppBundle;
        }
    }

    private static void PerformiOSBuild()
    {
        Debug.Log("[BuildAutomation] Iniciando Build iOS (Xcode)...");
        string previousBuildNumber = PlayerSettings.iOS.buildNumber;
        try
        {
            ConfigureGeneralSettings();
            ConfigureiOSSettings();

            string outputDir = Environment.GetEnvironmentVariable("FOXSTALE_IOS_OUTPUT_PATH");
            if (string.IsNullOrEmpty(outputDir))
                outputDir = Path.Combine(ProjectRoot, "Builds", "iOS", "XcodeProject");
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
        finally
        {
            PlayerSettings.iOS.buildNumber = previousBuildNumber;
        }
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

        // Target API 36 is the installed stable platform required for Play uploads.
        // API 37 is present only as a mismatched 37.0 SDK package and Gradle cannot resolve it.
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
        // Unity 6 requires API 26 or newer; maintain compatibility with the
        // supported modern Android baseline while targeting API 36.
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;

        // Arquitecturas 64-bit y 32-bit (ARM64 requerido por Google Play)
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

        // Permisos de Internet requeridos para Ads y Unity Gaming Services
        PlayerSettings.Android.forceInternetPermission = true;

        string versionCode = GetBuildSetting("FOXSTALE_ANDROID_VERSION_CODE");
        if (!string.IsNullOrEmpty(versionCode))
        {
            if (!int.TryParse(versionCode, out int parsedCode) || parsedCode < 1)
                throw new InvalidOperationException("FOXSTALE_ANDROID_VERSION_CODE debe ser un entero positivo.");
            PlayerSettings.Android.bundleVersionCode = parsedCode;
        }

        if (isAppBundle)
        {
            string keystorePath = GetBuildSetting("FOXSTALE_KEYSTORE_PATH");
            string keystorePassword = GetBuildSetting("FOXSTALE_KEYSTORE_PASSWORD");
            string alias = GetBuildSetting("FOXSTALE_KEYALIAS_NAME");
            string aliasPassword = GetBuildSetting("FOXSTALE_KEYALIAS_PASSWORD");
            if (string.IsNullOrEmpty(keystorePath) || !File.Exists(keystorePath) ||
                string.IsNullOrEmpty(keystorePassword) || string.IsNullOrEmpty(alias) ||
                string.IsNullOrEmpty(aliasPassword))
            {
                string missingSettings = string.Join(", ", new[]
                {
                    string.IsNullOrEmpty(keystorePath) || !File.Exists(keystorePath) ? "FOXSTALE_KEYSTORE_PATH" : null,
                    string.IsNullOrEmpty(keystorePassword) ? "FOXSTALE_KEYSTORE_PASSWORD" : null,
                    string.IsNullOrEmpty(alias) ? "FOXSTALE_KEYALIAS_NAME" : null,
                    string.IsNullOrEmpty(aliasPassword) ? "FOXSTALE_KEYALIAS_PASSWORD" : null
                }.Where(setting => !string.IsNullOrEmpty(setting)));
                throw new InvalidOperationException("Falta configuración local para firmar el AAB Android: " + missingSettings);
            }
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keyaliasPass = aliasPassword;
        }
    }

    private static void ConfigureiOSSettings()
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, APP_IDENTIFIER);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        string buildNumber = GetBuildSetting("FOXSTALE_IOS_BUILD_NUMBER");
        if (!string.IsNullOrEmpty(buildNumber))
        {
            if (!int.TryParse(buildNumber, out int parsedNumber) || parsedNumber < 1)
                throw new InvalidOperationException("FOXSTALE_IOS_BUILD_NUMBER debe ser un entero positivo.");
            PlayerSettings.iOS.buildNumber = buildNumber;
        }
    }

    private static string[] GetScenesForBuild()
    {
        List<string> scenes = new List<string>();

        // La subida no debe omitir niveles silenciosamente.
        foreach (string scenePath in RequiredScenes)
        {
            if (!File.Exists(Path.Combine(ProjectRoot, scenePath)))
                throw new InvalidOperationException("Falta una escena requerida: " + scenePath);
            scenes.Add(scenePath);
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
            throw new InvalidOperationException("Build de Unity falló; no se puede subir el artefacto.");
        }
        else
            throw new InvalidOperationException("Build de Unity no terminó correctamente.");
    }
}
