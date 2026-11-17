using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player appearance catalog: a style (accessories) and a color, each unlocked by
/// progress. Skin sheets live in Resources/PlayerSkins/{Style}_{Color} and reuse
/// the sprite names of the original sheets, so the existing Animator and clips
/// drive every skin.
/// </summary>
public static class PlayerSkins
{
    public static readonly string[] Styles = { "Classic", "Aviator" };
    public static readonly string[] Colors = { "Original", "Amber", "Arctic", "Smoke", "Crimson" };

    /// <summary>Levels finished with every gem that each additional style costs.</summary>
    public const int PerfectLevelsPerStyle = 2;
    /// <summary>Level clovers that each additional color costs.</summary>
    public const int CloversPerColor = 2;

    private static readonly Dictionary<Sprite, Sprite> resolved = new Dictionary<Sprite, Sprite>();
    private static readonly Dictionary<string, Sprite[]> sheets = new Dictionary<string, Sprite[]>();
    private static string resolvedFolder;

    public static int StyleIndex
    {
        get
        {
            int saved = SaveManager.Instance != null ? SaveManager.Instance.LoadSkinStyle() : 0;
            return saved >= 0 && saved < Styles.Length && IsStyleUnlocked(saved) ? saved : 0;
        }
    }

    public static int ColorIndex
    {
        get
        {
            int saved = SaveManager.Instance != null ? SaveManager.Instance.LoadSkinColor() : 0;
            return saved >= 0 && saved < Colors.Length && IsColorUnlocked(saved) ? saved : 0;
        }
    }

    public static int StyleRequirement(int index) => index * PerfectLevelsPerStyle;
    public static int ColorRequirement(int index) => index * CloversPerColor;

    public static bool IsStyleUnlocked(int index)
    {
        return index <= 0 || (SaveManager.Instance != null &&
                              SaveManager.Instance.CountPerfectGemLevels() >= StyleRequirement(index));
    }

    public static bool IsColorUnlocked(int index)
    {
        return index <= 0 || (SaveManager.Instance != null &&
                              SaveManager.Instance.CountLevelClovers() >= ColorRequirement(index));
    }

    /// <summary>Index of the first locked style, or -1 when all are unlocked.</summary>
    public static int NextLockedStyle()
    {
        for (int i = 0; i < Styles.Length; i++)
            if (!IsStyleUnlocked(i)) return i;
        return -1;
    }

    /// <summary>Index of the first locked color, or -1 when all are unlocked.</summary>
    public static int NextLockedColor()
    {
        for (int i = 0; i < Colors.Length; i++)
            if (!IsColorUnlocked(i)) return i;
        return -1;
    }

    /// <summary>Selects the next unlocked style, wrapping to the first.</summary>
    public static void SelectNextStyle()
    {
        if (SaveManager.Instance == null) return;
        int next = StyleIndex;
        do next = (next + 1) % Styles.Length; while (!IsStyleUnlocked(next));
        SaveManager.Instance.SaveSkinStyle(next);
    }

    /// <summary>Selects the next unlocked color, wrapping to the first.</summary>
    public static void SelectNextColor()
    {
        if (SaveManager.Instance == null) return;
        int next = ColorIndex;
        do next = (next + 1) % Colors.Length; while (!IsColorUnlocked(next));
        SaveManager.Instance.SaveSkinColor(next);
    }

    /// <summary>
    /// Returns the sprite of the selected skin that matches an original player
    /// sprite. Sprites that have no skinned counterpart are returned unchanged.
    /// </summary>
    public static Sprite Resolve(Sprite original)
    {
        if (original == null)
            return null;

        int style = StyleIndex;
        int color = ColorIndex;
        if (style == 0 && color == 0)
            return original;

        string folder = $"PlayerSkins/{Styles[style]}_{Colors[color]}";
        if (folder != resolvedFolder)
        {
            resolved.Clear();
            resolvedFolder = folder;
        }

        if (resolved.TryGetValue(original, out Sprite skinned))
            return skinned;

        skinned = original;
        if (original.texture != null)
        {
            string sheetPath = $"{folder}/{original.texture.name}";
            if (!sheets.TryGetValue(sheetPath, out Sprite[] sheet))
            {
                sheet = Resources.LoadAll<Sprite>(sheetPath);
                sheets[sheetPath] = sheet;
            }

            for (int i = 0; i < sheet.Length; i++)
            {
                if (sheet[i].name == original.name)
                {
                    skinned = sheet[i];
                    break;
                }
            }
        }

        resolved[original] = skinned;
        return skinned;
    }
}
