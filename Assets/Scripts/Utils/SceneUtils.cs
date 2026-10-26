using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utilidades estáticas de carga de escena.
/// Todas las cargas pasan por SceneTransitionManager para garantizar
/// que siempre haya un fade out antes de cambiar de escena.
/// Si SceneTransitionManager no está disponible (e.g. primeros frames),
/// hace fallback a SceneManager directo.
/// </summary>
public class SceneUtils : MonoBehaviour
{
    // ───────────────────────────────────────────────
    //  Navegación
    // ───────────────────────────────────────────────

    public static void LoadGameplayScene(int level)
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadGameplayScene(level);
        else
            SceneManager.LoadScene("Level_" + level);
    }

    public static void ToSelectionScene()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.ToSelectionScene();
        else
            SceneManager.LoadScene("LevelSelection");
    }

    public static void ToMainScene()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.ToMainScene();
        else
            SceneManager.LoadScene("MainMenu");
    }

    public static void LoadScene(string sceneName)
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    public static void LoadScene(int buildIndex)
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(buildIndex);
        else
            SceneManager.LoadScene(buildIndex);
    }

    public static void NextScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ───────────────────────────────────────────────
    //  Queries del estado de la escena actual
    // ───────────────────────────────────────────────

    public static bool IsInGameplay()
    {
        return SceneManager.GetActiveScene().name.StartsWith("Level_",
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsInTitleScreen()
    {
        return SceneManager.GetActiveScene().name.Equals("MainMenu",
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Retorna el número del nivel actual. Retorna -1 si el parseo falla.
    /// </summary>
    public static int Get_CurrentLevelName()
    {
        return ParseLevelNumber(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Retorna el número del siguiente nivel. Retorna -1 si el parseo falla.
    /// </summary>
    public static int Get_NextLevelName()
    {
        int current = ParseLevelNumber(SceneManager.GetActiveScene().name);
        return current >= 0 ? current + 1 : -1;
    }

    // ───────────────────────────────────────────────
    //  Helpers privados
    // ───────────────────────────────────────────────

    /// <summary>
    /// Parsea el número de nivel del nombre de escena con formato "Level_X".
    /// Retorna -1 en lugar de lanzar excepción si el formato no es válido.
    /// </summary>
    private static int ParseLevelNumber(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneUtils] Nombre de escena vacío al intentar parsear número de nivel.");
            return -1;
        }

        string[] parts = sceneName.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out int number))
            return number;

        Debug.LogWarning($"[SceneUtils] No se pudo parsear número de nivel del nombre '{sceneName}'. " +
                         "Asegúrate de que el nombre de escena tenga el formato 'Level_N'.");
        return -1;
    }
}
