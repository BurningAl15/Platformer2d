using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Reports unlocked achievements to a platform service (Google Play Games, Game Center, Steam).</summary>
public interface IAchievementReporter
{
    void Report(string achievementId);
}

/// <summary>
/// Game-side achievements and unlock notices. Definitions are evaluated
/// against saved progress, so calling <see cref="Evaluate"/> after any change
/// is safe and idempotent. Platform services plug in as reporters.
/// </summary>
public static class Achievements
{
    public sealed class Definition
    {
        public readonly string Id;
        public readonly string NameKey;
        public readonly NotificationCenter.IconKind Icon;
        public readonly Func<SaveManager, bool> IsMet;
        /// <summary>Optional count toward <see cref="Goal"/>, shown as progress in the list.</summary>
        public readonly Func<SaveManager, int> Progress;
        public readonly int Goal;

        public string DescriptionKey => NameKey + "_DESC";

        public Definition(string id, string nameKey, NotificationCenter.IconKind icon, Func<SaveManager, bool> isMet,
            Func<SaveManager, int> progress = null, int goal = 0)
        {
            Id = id; NameKey = nameKey; Icon = icon; IsMet = isMet; Progress = progress; Goal = goal;
        }
    }

    /// <summary>Story levels in World 1.</summary>
    public const int WorldLevelCount = 8;

    public static readonly Definition[] All =
    {
        new Definition("tutorial_complete", "ACH_TUTORIAL", NotificationCenter.IconKind.Fox, s => s.IsTutorialCompleted()),
        new Definition("first_level", "ACH_FIRST_LEVEL", NotificationCenter.IconKind.Star, s => s.HasFinishedLevel(1)),
        new Definition("first_clover", "ACH_FIRST_CLOVER", NotificationCenter.IconKind.Clover, s => s.CountLevelClovers(WorldLevelCount) >= 1),
        new Definition("first_perfect", "ACH_FIRST_PERFECT", NotificationCenter.IconKind.Gem, s => s.CountPerfectGemLevels(WorldLevelCount) >= 1),
        new Definition("three_stars", "ACH_THREE_STARS", NotificationCenter.IconKind.Star, s => AnyLevel(s, n => s.LoadRating(n) >= 3)),
        new Definition("first_heart", "ACH_FIRST_HEART", NotificationCenter.IconKind.Heart, s => s.LoadPermanentHeartCount() >= 1),
        new Definition("world_1", "ACH_WORLD_1", NotificationCenter.IconKind.Star, s => s.HasFinishedLevel(WorldLevelCount)),
        new Definition("all_clovers", "ACH_ALL_CLOVERS", NotificationCenter.IconKind.Clover, s => s.CountLevelClovers(WorldLevelCount) >= WorldLevelCount,
            s => s.CountLevelClovers(WorldLevelCount), WorldLevelCount),
        new Definition("all_perfect", "ACH_ALL_PERFECT", NotificationCenter.IconKind.Gem, s => s.CountPerfectGemLevels(WorldLevelCount) >= WorldLevelCount,
            s => s.CountPerfectGemLevels(WorldLevelCount), WorldLevelCount),
    };

    private const string SeededFlag = "Achievements_Seeded";
    private static readonly List<IAchievementReporter> reporters = new List<IAchievementReporter>();

    public static void AddReporter(IAchievementReporter reporter)
    {
        if (reporter != null && !reporters.Contains(reporter)) reporters.Add(reporter);
    }

    public static bool IsUnlocked(string id) =>
        SaveManager.Instance != null && SaveManager.Instance.GetFlag("Achievement_" + id);

    public static int UnlockedCount()
    {
        int count = 0;
        foreach (Definition definition in All)
            if (IsUnlocked(definition.Id)) count++;
        return count;
    }

    /// <summary>
    /// Unlocks every achievement whose condition is met and announces new
    /// achievements and newly unlocked styles and colors. The first run only
    /// records progress made before the system existed, without notices.
    /// </summary>
    public static void Evaluate()
    {
        SaveManager save = SaveManager.Instance;
        if (save == null) return;
        bool announce = save.GetFlag(SeededFlag);

        foreach (Definition definition in All)
        {
            string key = "Achievement_" + definition.Id;
            if (save.GetFlag(key) || !definition.IsMet(save)) continue;
            save.SetFlag(key);
            foreach (IAchievementReporter reporter in reporters) reporter.Report(definition.Id);
            if (announce)
                NotificationCenter.Show(GameLocalization.Get("NOTIFY_ACHIEVEMENT"),
                    GameLocalization.Get(definition.NameKey), definition.Icon);
        }

        for (int i = 1; i < PlayerSkins.Styles.Length; i++)
            AnnounceUnlock(save, "Unlock_Style_" + i, PlayerSkins.IsStyleUnlocked(i), announce,
                "NOTIFY_NEW_STYLE", "SKIN_STYLE_" + PlayerSkins.Styles[i].ToUpperInvariant(), i, 0);
        for (int i = 1; i < PlayerSkins.Colors.Length; i++)
            AnnounceUnlock(save, "Unlock_Color_" + i, PlayerSkins.IsColorUnlocked(i), announce,
                "NOTIFY_NEW_COLOR", "SKIN_COLOR_" + PlayerSkins.Colors[i].ToUpperInvariant(), 0, i);

        if (!announce) save.SetFlag(SeededFlag);
    }

    private static void AnnounceUnlock(SaveManager save, string key, bool unlocked, bool announce,
        string titleKey, string nameKey, int style, int color)
    {
        if (!unlocked || save.GetFlag(key)) return;
        save.SetFlag(key);
        if (!announce) return;
        NotificationCenter.Show(GameLocalization.Get(titleKey), GameLocalization.Get(nameKey),
            NotificationCenter.IconKind.Fox, style, color);
    }

    private static bool AnyLevel(SaveManager save, Func<int, bool> test)
    {
        for (int n = 1; n <= WorldLevelCount; n++)
            if (test(n)) return true;
        return false;
    }
}
