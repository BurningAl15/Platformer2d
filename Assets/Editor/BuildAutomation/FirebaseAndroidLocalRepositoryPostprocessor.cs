#if UNITY_EDITOR
using System.IO;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// Fixes EDM's generated Firebase Maven repository URI for macOS project paths.
/// The resolver emits file:////Users/... which Gradle cannot convert to a File.
/// </summary>
public sealed class FirebaseAndroidLocalRepositoryPostprocessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 10000;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string settingsPath = null;
        DirectoryInfo directory = new DirectoryInfo(path);
        for (int depth = 0; directory != null && depth < 6; depth++, directory = directory.Parent)
        {
            string candidate = Path.Combine(directory.FullName, "settings.gradle");
            if (File.Exists(candidate))
            {
                settingsPath = candidate;
                break;
            }
        }

        if (settingsPath == null)
            return;

        string[] lines = File.ReadAllLines(settingsPath);
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string firebaseRepository = Path.GetFullPath(Path.Combine(
            projectRoot, "Assets", "GeneratedLocalRepo", "Firebase", "m2repository"));
        string groovyPath = firebaseRepository.Replace("\\", "\\\\").Replace("\"", "\\\"");
        bool changed = false;

        for (int i = 0; i < lines.Length; i++)
        {
            if (!lines[i].Contains("GeneratedLocalRepo/Firebase/m2repository"))
                continue;

            int indentLength = lines[i].Length - lines[i].TrimStart().Length;
            string indent = lines[i].Substring(0, indentLength);
            lines[i] = indent + "url = new File(\"" + groovyPath + "\").toURI()";
            changed = true;
        }

        if (!changed)
            return;

        File.WriteAllLines(settingsPath, lines);
        Debug.Log("[Firebase] Normalized local Maven repository path in generated Gradle settings.");
    }
}
#endif
