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

    private static readonly Dictionary<string, Dictionary<Sprite, Sprite>> resolved =
        new Dictionary<string, Dictionary<Sprite, Sprite>>();
    private static readonly Dictionary<string, Sprite[]> sheets = new Dictionary<string, Sprite[]>();

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

    /// <summary>Saves the selection when both options are unlocked.</summary>
    public static bool TrySelect(int style, int color)
    {
        if (SaveManager.Instance == null || style < 0 || style >= Styles.Length || color < 0 || color >= Colors.Length ||
            !IsStyleUnlocked(style) || !IsColorUnlocked(color))
            return false;

        SaveManager.Instance.SaveSkinStyle(style);
        SaveManager.Instance.SaveSkinColor(color);
        return true;
    }

    /// <summary>
    /// Returns the sprite of the selected skin that matches an original player
    /// sprite. Sprites that have no skinned counterpart are returned unchanged.
    /// </summary>
    public static Sprite Resolve(Sprite original)
    {
        return ResolveFor(original, StyleIndex, ColorIndex);
    }

    /// <summary>Same as <see cref="Resolve"/> for any style and color, locked or not.</summary>
    public static Sprite ResolveFor(Sprite original, int style, int color)
    {
        if (original == null)
            return null;
        if (style <= 0 && color <= 0)
            return original;

        string folder = $"PlayerSkins/{Styles[style]}_{Colors[color]}";
        if (!resolved.TryGetValue(folder, out Dictionary<Sprite, Sprite> map))
        {
            map = new Dictionary<Sprite, Sprite>();
            resolved[folder] = map;
        }

        if (map.TryGetValue(original, out Sprite skinned))
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

        map[original] = skinned;
        return skinned;
    }
}
