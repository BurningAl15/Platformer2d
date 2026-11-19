using UnityEngine;

/// <summary>
/// What the current platform offers, decided in one place. The profile follows
/// the build target (in the Editor, the active build target), so the same build
/// pipeline produces the right menus on mobile, PC, consoles and the web.
/// </summary>
public static class PlatformFeatures
{
    public enum Profile { Mobile, Desktop, Console, Web }

#if UNITY_EDITOR
    /// <summary>Editor-only: preview another platform's menus without switching the build target.</summary>
    public static Profile? EditorOverride;
#endif

    public static Profile Current
    {
        get
        {
#if UNITY_EDITOR
            if (EditorOverride.HasValue) return EditorOverride.Value;
#endif
#if UNITY_ANDROID || UNITY_IOS
            return Profile.Mobile;
#elif UNITY_WEBGL
            return Profile.Web;
#elif UNITY_STANDALONE
            return Profile.Desktop;
#else
            return Application.isConsolePlatform ? Profile.Console : Profile.Desktop;
#endif
        }
    }

    public static bool IsMobile => Current == Profile.Mobile;

    /// <summary>Rewarded ads and the paid "no ads" store exist only in the mobile builds.</summary>
    public static bool HasStore => IsMobile;

    public static bool HasVibration => IsMobile;

    public static bool HasKeepScreenAwake => IsMobile;

    /// <summary>Fullscreen / window switch.</summary>
    public static bool HasDisplayModeOption => Current == Profile.Desktop || Current == Profile.Web;

    /// <summary>iOS, consoles and browsers do not allow an app to quit itself.</summary>
    public static bool HasQuitButton
    {
        get
        {
#if UNITY_IOS
            if (Current == Profile.Mobile) return false;
#endif
            return Current == Profile.Desktop || Current == Profile.Mobile;
        }
    }

    private const string FullscreenKey = "DisplayFullscreen";

    public static bool IsFullscreenPreferred => PlayerPrefs.GetInt(FullscreenKey, 1) == 1;

    public static void SetFullscreen(bool fullscreen)
    {
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
        ApplyDisplayPreference();
    }

    /// <summary>Applies the saved fullscreen choice on PC (the web needs a click, so it is applied from the toggle only).</summary>
    public static void ApplyDisplayPreference()
    {
        if (!HasDisplayModeOption) return;
        Screen.fullScreenMode = IsFullscreenPreferred ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }
}
