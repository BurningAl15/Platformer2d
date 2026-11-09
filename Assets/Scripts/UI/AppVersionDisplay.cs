using TMPro;
using UnityEngine;

/// <summary>
/// Muestra la versión del juego y el número de compilación (build number) en pantalla.
/// Ejemplo: "v1.0.1 (b11)"
/// Obtiene el número de compilación desde:
/// 1. Resources/build_info.txt (inyectado durante el build)
/// 2. Android JNI PackageInfo (en dispositivos Android nativos)
/// 3. Fallback a PlayerSettings / Application.version
/// </summary>
public class AppVersionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private string prefix = "v";

    private void Awake()
    {
        if (versionText == null)
            versionText = GetComponent<TextMeshProUGUI>();

        UpdateDisplay();
    }

    private void Start()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (versionText != null)
        {
            versionText.text = GetFormattedVersion(prefix);
        }
    }

    public static string GetBuildNumber()
    {
        // 1. Intentar cargar desde Resources/build_info
        try
        {
            TextAsset asset = Resources.Load<TextAsset>("build_info");
            if (asset != null && !string.IsNullOrWhiteSpace(asset.text))
            {
                return asset.text.Trim();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[AppVersionDisplay] Error leyendo build_info.txt: " + ex.Message);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // 2. En Android real, leer versionCode del PackageManager
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (currentActivity != null)
                {
                    string packageName = currentActivity.Call<string>("getPackageName");
                    AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                    AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);

                    long versionCode = 0;
                    try
                    {
                        versionCode = packageInfo.Call<long>("getLongVersionCode");
                    }
                    catch
                    {
                        versionCode = packageInfo.Get<int>("versionCode");
                    }

                    if (versionCode > 0)
                        return versionCode.ToString();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[AppVersionDisplay] Error leyendo versionCode Android: " + ex.Message);
        }
#endif

        return string.Empty;
    }

    public static string GetFormattedVersion(string prefix = "v")
    {
        string version = Application.version;
        if (string.IsNullOrEmpty(version))
            version = "1.0.0";

        string build = GetBuildNumber();
        if (!string.IsNullOrEmpty(build))
        {
            return $"{prefix}{version} (b{build})";
        }
        return $"{prefix}{version}";
    }
}
