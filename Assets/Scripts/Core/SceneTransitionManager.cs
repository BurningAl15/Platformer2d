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
        StartCoroutine(TransitionRoutine(() => SceneManager.LoadScene(sceneName)));
    }

    public void LoadScene(int buildIndex)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(() => SceneManager.LoadScene(buildIndex)));
    }

    public void LoadGameplayScene(int level)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(() => SceneManager.LoadScene("Level_" + level)));
    }

    public void ToSelectionScene()
    {
        LoadScene("LevelSelection");
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
