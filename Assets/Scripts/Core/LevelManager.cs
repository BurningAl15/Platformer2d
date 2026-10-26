using System.Collections;
using UnityEngine;

/// <summary>
/// Orquesta el ciclo de vida de un nivel de gameplay:
/// - Respawn del jugador.
/// - Fin de nivel (secuencia de animación, guardado, transición).
/// - Conteo de gemas y tiempo de juego.
///
/// El guardado de datos delega a SaveManager.
/// Las analíticas delegan a AnalyticsManager.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager _instance;

    [Header("Respawn")]
    [SerializeField] private float respawnHoldTime = 0.5f;

    [Header("Estado del nivel")]
    public bool stopGame = false;
    [Tooltip("Total de gemas del nivel. Si es 0, se detecta automáticamente al iniciar buscando Pickups de tipo Gema.")]
    [SerializeField] private int totalGemsInLevel = 0;

    private int gemsCollected = 0;
    public float timeInLevel { get; private set; }

    private Coroutine currentCoroutine = null;

    // ───────────────────────────────────────────────
    //  Ciclo de vida
    // ───────────────────────────────────────────────

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        timeInLevel = 0f;

        if (totalGemsInLevel <= 0)
        {
            Pickup[] pickups = FindObjectsOfType<Pickup>();
            for (int i = 0; i < pickups.Length; i++)
            {
                if (pickups[i] != null && pickups[i].IsGem())
                    totalGemsInLevel++;
            }
        }

        UIController._instance?.UpdateGems_UI(gemsCollected, totalGemsInLevel);

        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.TrackLevelStart(SceneUtils.Get_CurrentLevelName());
    }

    private void Update()
    {
        if (!stopGame)
            timeInLevel += Time.deltaTime;
    }

    // ───────────────────────────────────────────────
    //  Gemas
    // ───────────────────────────────────────────────

    public void UpdateGemsCollected()
    {
        gemsCollected++;
        UIController._instance?.UpdateGems_UI(gemsCollected, totalGemsInLevel);

        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.TrackGemCollected(SceneUtils.Get_CurrentLevelName(), gemsCollected);
    }

    public int GetGemsCollected() => gemsCollected;
    public int GetTotalGemsInLevel() => totalGemsInLevel;

    // ───────────────────────────────────────────────
    //  Respawn
    // ───────────────────────────────────────────────

    /// <summary>
    /// Llamado cuando el jugador muere (currentHealth = 0).
    /// Consulta LivesManager: si quedan vidas respawnea; si no, activa Game Over.
    /// </summary>
    public void RespawnPlayer()
    {
        if (currentCoroutine != null) return;

        // Consumir una vida y verificar si el jugador puede continuar
        bool canContinue = LivesManager.Instance == null || LivesManager.Instance.ConsumeLifeAndCheck();

        if (canContinue)
        {
            currentCoroutine = StartCoroutine(RespawnRoutine());
        }
        // Si no puede continuar, LivesManager dispara OnGameOver y
        // GameOverController lo maneja. No iniciamos la coroutine.
    }

    private IEnumerator RespawnRoutine()
    {
        PlayerHealth._instance.DeathEffect();
        PlayerHealth._instance.TurnOffPlayer();
        yield return new WaitUntil(() => PlayerHealth._instance.isDeathEffectEnd());
        FadeEffect._instance.Fade_Out();
        yield return new WaitUntil(() => FadeEffect._instance.endFade);
        yield return new WaitForSeconds(respawnHoldTime);
        PlayerHealth._instance.ResetHealth();
        UIController._instance.ResetHearts();
        PlayerHealth._instance.ResetPlayer();
        PlayerHealth._instance.ResetDeathEffect();
        FadeEffect._instance.Fade_In();
        currentCoroutine = null;
    }

    // ───────────────────────────────────────────────
    //  Fin de nivel
    // ───────────────────────────────────────────────

    public void EndLevel()
    {
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(EndLevelRoutine());
    }

    private IEnumerator EndLevelRoutine()
    {
        stopGame = true;

        AudioMixerManager._instance.PlayBackgroundSource(false);
        SimpleCameraController2d._instance.StopFollow();

        yield return new WaitForSeconds(0.5f);
        UIController._instance.Run_EndLevelAnimation();
        yield return new WaitForSeconds(1.5f);

        FadeEffect._instance.Fade_Out();

        // Guardar progreso (delegado a SaveManager)
        LevelRating rating = SaveProgress();

        yield return new WaitUntil(() => FadeEffect._instance.endFade);
        yield return new WaitForSeconds(0.25f);

        // Si recolectó todas las gemas → mostrar pantalla del dado antes de ir al mapa
        bool allGemsCollected = totalGemsInLevel > 0 && gemsCollected >= totalGemsInLevel;
        if (allGemsCollected && DiceRollController.Instance != null)
        {
            FadeEffect._instance.Fade_In();
            yield return new WaitUntil(() => FadeEffect._instance.endFade);
            DiceRollController.Instance.Show(DiceType.Normal);
            // El flujo continúa cuando DiceRollController llame SceneUtils.ToSelectionScene()
            // via su botón "Continuar". El LevelManager cede el control aquí.
            currentCoroutine = null;
            yield break;
        }

        SceneUtils.ToSelectionScene();
    }

    private LevelRating SaveProgress()
    {
        int levelNumber = SceneUtils.Get_CurrentLevelName();

        // Siempre desbloquear el siguiente nivel
        if (SaveManager.Instance != null)
            SaveManager.Instance.UnlockNextLevel(levelNumber);

        LevelRating rating = LevelRating.OneStar;

        // Guardar gemas y tiempo solo en niveles reales (no el nivel 0 que es el tutorial)
        if (levelNumber > 0 && SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGemsIfBetter(levelNumber, gemsCollected);
            SaveManager.Instance.SaveTimeIfBetter(levelNumber, timeInLevel);
            SaveManager.Instance.AddGlobalGems(gemsCollected);
        }

        // Calcular y guardar rating de estrellas
        if (LevelRatingSystem._instance != null)
        {
            var req = new LevelRatingSystem.LevelRequirements
            {
                targetTime = SaveManager.Instance != null
                    ? SaveManager.Instance.LoadBestTime(levelNumber)
                    : 99999f,
                totalGems = totalGemsInLevel > 0 ? totalGemsInLevel : gemsCollected
            };
            rating = LevelRatingSystem._instance.CalculateRating(timeInLevel, gemsCollected, req);
            SaveManager.Instance?.SaveRatingIfBetter(levelNumber, (int)rating);
        }

        UIController._instance?.ShowLevelRating(rating);

        // Analíticas
        if (levelNumber > 0 && AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.TrackLevelComplete(levelNumber, timeInLevel, gemsCollected);

        return rating;
    }
}
