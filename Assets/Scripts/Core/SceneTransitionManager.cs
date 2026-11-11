using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona TODAS las transiciones de escena del juego con fade in/out.
/// Reemplaza las llamadas directas a SceneManager.LoadScene en SceneUtils,
/// PauseMenu y MainMenuManager.
///
/// Uso:
///   SceneTransitionManager.Instance.LoadScene("NombreEscena");
///   SceneTransitionManager.Instance.LoadScene(buildIndex);
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Tooltip("Duración del fade out antes de cargar (segundos)")]
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Tooltip("Pausa entre fade out y carga de escena (segundos)")]
    [SerializeField] private float holdDuration = 0.1f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ───────────────────────────────────────────────
    //  API pública
    // ───────────────────────────────────────────────

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;

        if (IsTutorialIncomplete() && IsRestrictedBeforeTutorial(sceneName))
        {
            LoadGameplayScene(0);
            return;
        }

        StartCoroutine(TransitionRoutine(() => SceneManager.LoadScene(sceneName)));
    }

    public void LoadScene(int buildIndex)
    {
        if (isTransitioning) return;

        // Integer-based callers (for example NextScene) must obey the same
        // first-run tutorial gate as callers that load by scene name.
        string targetScene = SceneManager.GetSceneByBuildIndex(buildIndex).name;
        if (IsTutorialIncomplete() && IsRestrictedBeforeTutorial(targetScene))
        {
            LoadGameplayScene(0);
            return;
        }

        StartCoroutine(TransitionRoutine(() => SceneManager.LoadScene(buildIndex)));
    }

    public void LoadGameplayScene(int level)
    {
        if (isTransitioning) return;

        // Never allow gameplay routes to bypass an unfinished first-run tutorial.
        if (level > 0 && IsTutorialIncomplete())
            level = 0;

        StartCoroutine(TransitionRoutine(() => SceneManager.LoadScene("Level_" + level)));
    }

    public void ToSelectionScene()
    {
        // Pause/menu routes must not expose the level map before the tutorial is complete.
        if (IsTutorialIncomplete())
        {
            LoadGameplayScene(0);
            return;
        }

        LoadScene("LevelSelection");
    }

    private static bool IsTutorialIncomplete()
    {
        if (SaveManager.Instance != null)
            return SaveManager.Instance.HasIncompleteTutorial();

        bool started = PlayerPrefs.GetInt("TutorialStarted", 0) == 1;
        bool completed = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1 ||
                         PlayerPrefs.GetInt("Level_1", 0) == 1;
        return started && !completed;
    }

    private static bool IsRestrictedBeforeTutorial(string sceneName)
    {
        if (string.Equals(sceneName, "LevelSelection", System.StringComparison.OrdinalIgnoreCase))
            return true;

        return sceneName.StartsWith("Level_", System.StringComparison.OrdinalIgnoreCase) &&
               int.TryParse(sceneName.Substring("Level_".Length), out int level) && level > 0;
    }

    public void ToMainScene()
    {
        LoadScene("MainMenu");
    }

    // ───────────────────────────────────────────────
    //  Coroutine interna
    // ───────────────────────────────────────────────

    private IEnumerator TransitionRoutine(System.Action loadAction)
    {
        isTransitioning = true;

        // Si la pantalla ya está en negro por un fade previo (ej. al terminar nivel), no repetir el fade
        if (FadeEffect._instance != null)
        {
            if (!FadeEffect._instance.IsScreenBlack())
            {
                FadeEffect._instance.Fade_Out();
                yield return new WaitUntil(() => FadeEffect._instance.endFade);
            }
        }
        else
        {
            // Sin FadeEffect: esperar la duración configurada como fallback
            yield return new WaitForSeconds(fadeOutDuration);
        }

        yield return new WaitForSeconds(holdDuration);

        loadAction?.Invoke();
        isTransitioning = false;
    }
}
