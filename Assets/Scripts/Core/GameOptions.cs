using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player options beyond the basic toggles: volume levels, camera shake,
/// gamepad rumble and PC video settings. Values live in PlayerPrefs and
/// survive a progress reset.
/// </summary>
public static class GameOptions
{
    public const int MaxVolume = 10;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SFXVolume";
    private const string ShakeKey = "ScreenShakeEnabled";
    private const string RumbleKey = "GamepadRumbleEnabled";
    private const string VSyncKey = "VSyncEnabled";
    private const string ResolutionKey = "ResolutionWxH";

    private static readonly string[] Keys = { MusicVolumeKey, SfxVolumeKey, ShakeKey, RumbleKey, VSyncKey, ResolutionKey, "DisplayFullscreen", "HapticsEnabled", "KeepScreenAwake" };

    // Volume: 0 mutes. Older saves only stored on/off (MusicLevel / SFXLevel).
    public static int MusicVolume
    {
        get => PlayerPrefs.GetInt(MusicVolumeKey, PlayerPrefs.GetInt("MusicLevel", 1) == 1 ? 8 : 0);
        set { PlayerPrefs.SetInt(MusicVolumeKey, Mathf.Clamp(value, 0, MaxVolume)); PlayerPrefs.SetInt("MusicLevel", value > 0 ? 1 : 0); PlayerPrefs.Save(); ApplyAudio(); }
    }

    public static int SfxVolume
    {
        get => PlayerPrefs.GetInt(SfxVolumeKey, PlayerPrefs.GetInt("SFXLevel", 1) == 1 ? 8 : 0);
        set { PlayerPrefs.SetInt(SfxVolumeKey, Mathf.Clamp(value, 0, MaxVolume)); PlayerPrefs.SetInt("SFXLevel", value > 0 ? 1 : 0); PlayerPrefs.Save(); ApplyAudio(); }
    }

    public static bool ScreenShake
    {
        get => PlayerPrefs.GetInt(ShakeKey, 1) == 1;
        set { PlayerPrefs.SetInt(ShakeKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static bool GamepadRumble
    {
        get => PlayerPrefs.GetInt(RumbleKey, 1) == 1;
        set { PlayerPrefs.SetInt(RumbleKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static bool VSync
    {
        get => PlayerPrefs.GetInt(VSyncKey, 1) == 1;
        set { PlayerPrefs.SetInt(VSyncKey, value ? 1 : 0); PlayerPrefs.Save(); ApplyVideo(); }
    }

    private static void ApplyAudio()
    {
        if (AudioMixerManager._instance != null) AudioMixerManager._instance.RefreshVolumes();
    }

    // ── PC video ──

    /// <summary>Distinct screen sizes the display supports, smallest first.</summary>
    public static List<Vector2Int> Resolutions()
    {
        var list = new List<Vector2Int>();
        foreach (Resolution r in Screen.resolutions)
        {
            var size = new Vector2Int(r.width, r.height);
            if (!list.Contains(size)) list.Add(size);
        }
        if (list.Count == 0) list.Add(new Vector2Int(Screen.width, Screen.height));
        return list;
    }

    public static Vector2Int CurrentResolution
    {
        get
        {
            string saved = PlayerPrefs.GetString(ResolutionKey, "");
            string[] parts = saved.Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h)) return new Vector2Int(w, h);
            return new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
        }
    }

    /// <summary>Moves to the next (or previous) supported resolution and applies it.</summary>
    public static void StepResolution(int direction)
    {
        List<Vector2Int> sizes = Resolutions();
        int index = Mathf.Max(0, sizes.IndexOf(CurrentResolution));
        index = (index + direction + sizes.Count) % sizes.Count;
        PlayerPrefs.SetString(ResolutionKey, $"{sizes[index].x}x{sizes[index].y}");
        PlayerPrefs.Save();
        ApplyVideo();
    }

    /// <summary>VSync everywhere; resolution and window mode only on PC.</summary>
    public static void ApplyVideo()
    {
        QualitySettings.vSyncCount = VSync ? 1 : 0;
        if (PlatformFeatures.Current != PlatformFeatures.Profile.Desktop) return;
        Vector2Int size = CurrentResolution;
        FullScreenMode mode = PlatformFeatures.IsFullscreenPreferred ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(size.x, size.y, mode);
    }

    // ── Rumble ──

    /// <summary>A short gamepad pulse, when a real gamepad is in use and rumble is on.</summary>
    public static void PulseGamepad(MonoBehaviour host, float strength = 0.45f, float seconds = 0.12f)
    {
        if (!GamepadRumble || host == null) return;
        Gamepad pad = Gamepad.current;
        if (pad == null || !MobileGameplayControls.IsPhysicalGameController(pad)) return;
        host.StartCoroutine(Pulse(pad, strength, seconds));
    }

    private static IEnumerator Pulse(Gamepad pad, float strength, float seconds)
    {
        pad.SetMotorSpeeds(strength * 0.6f, strength);
        yield return new WaitForSecondsRealtime(seconds);
        pad.ResetHaptics();
    }

    // ── Reset progress keeps options ──

    public static Dictionary<string, string> Snapshot()
    {
        var values = new Dictionary<string, string>();
        foreach (string key in Keys)
            if (PlayerPrefs.HasKey(key))
                values[key] = PlayerPrefs.GetString(key, null) ?? PlayerPrefs.GetInt(key).ToString();
        return values;
    }

    public static void Restore(Dictionary<string, string> values)
    {
        foreach (var pair in values)
        {
            if (pair.Key == ResolutionKey) PlayerPrefs.SetString(pair.Key, pair.Value);
            else if (int.TryParse(pair.Value, out int number)) PlayerPrefs.SetInt(pair.Key, number);
        }
        PlayerPrefs.Save();
    }
}
