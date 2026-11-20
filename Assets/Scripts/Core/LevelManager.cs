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

    [Header("Resumen de mundo (opcional)")]
    [SerializeField] private WorldData currentWorldData;
    [SerializeField] private WorldCompleteController worldCompleteController;

    private int gemsCollected = 0;
    public float timeInLevel { get; private set; }

    // A clover picked up in this run is always new: collected clovers are hidden on later runs.
    private bool cloverFoundThisRun;
    private int livesLostThisRun;
    private int damageTakenThisRun;

    /// <summary>Called by PlayerHealth whenever the player loses health (hits and falls).</summary>
    public void NotifyDamageTaken() => damageTakenThisRun++;

    /// <summary>True while the player has taken no damage at all in this run of the level.</summary>
    public bool IsFlawlessRun => damageTakenThisRun == 0 && livesLostThisRun == 0;
    private bool earnedRewardRoll;

    public void NotifyCloverFound() => cloverFoundThisRun = true;

    /// <summary>Time that earns the level's time star, or 0 when the level has none.</summary>
    public float TargetTime => currentWorldData != null ? currentWorldData.GetTargetTime(gameObject.scene.name) : 0f;

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

        if (SceneUtils.Get_CurrentLevelName() == 0)
            SaveManager.Instance?.MarkTutorialStarted();

        // Derive the target from scene contents on every run instead of trusting
        // a stale Inspector number. Include inactive pickups so hidden/late-area
        // gems remain part of the level total, and ignore objects in other scenes.
        totalGemsInLevel = CountGemsInThisScene();
        Debug.Log($"[LevelManager] {gameObject.scene.name}: {totalGemsInLevel} gem pickups in scene.");

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

    public void UpdateGemsCollected(bool refreshHud = true)
    {
        if (gemsCollected >= totalGemsInLevel)
        {
            Debug.LogWarning($"[LevelManager] Ignored extra gem event ({gemsCollected}/{totalGemsInLevel}) in {gameObject.scene.name}.");
            return;
        }

        gemsCollected++;
        if (refreshHud)
            UIController._instance?.UpdateGems_UI(gemsCollected, totalGemsInLevel);

        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.TrackGemCollected(SceneUtils.Get_CurrentLevelName(), gemsCollected);
    }

    private int CountGemsInThisScene()
    {
        int count = 0;
        Pickup[] pickups = FindObjectsOfType<Pickup>(true);
        foreach (Pickup pickup in pickups)
        {
            if (pickup != null && pickup.gameObject.scene == gameObject.scene && pickup.IsGem())
                count++;
        }
        return count;
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
        TryRespawn(consumeLife: true, restoreHealth: true);
    }

    /// <summary>La caída quita salud y vuelve al waypoint sin consumir una vida global.</summary>
    public void RespawnPlayerFromFall()
    {
        TryRespawn(consumeLife: false, restoreHealth: false);
    }

    /// <summary>Continuar desde Game Over tras recuperar vidas restaura la salud.</summary>
    public void RespawnPlayerAfterContinue()
    {
        TryRespawn(consumeLife: false, restoreHealth: true);
    }

    private void TryRespawn(bool consumeLife, bool restoreHealth)
    {
        if (currentCoroutine != null) return;

        bool isTutorial = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level_0";
        bool canContinue = !consumeLife || isTutorial || LivesManager.Instance == null ||
                           LivesManager.Instance.ConsumeLifeAndCheck();
        if (consumeLife) livesLostThisRun++;

        if (canContinue)
            currentCoroutine = StartCoroutine(RespawnRoutine(restoreHealth));
        // Si no puede continuar, LivesManager dispara OnGameOver y
        // GameOverController lo maneja. No iniciamos la coroutine.
    }

    private IEnumerator RespawnRoutine(bool restoreHealth)
    {
        if (PlayerHealth._instance == null) { currentCoroutine = null; yield break; }

        PlayerHealth._instance.DeathEffect();
        PlayerHealth._instance.TurnOffPlayer();
        yield return new WaitUntil(() => PlayerHealth._instance.isDeathEffectEnd());
        FadeEffect._instance.Fade_Out();
        yield return new WaitUntil(() => FadeEffect._instance.endFade);
        yield return new WaitForSeconds(respawnHoldTime);
        if (restoreHealth)
            PlayerHealth._instance.ResetHealth();
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

        // Save progress before opening any reward or completion panel.
        LevelRating rating = SaveProgress();

        // Si recolectó todas las gemas → mostrar pantalla del dado antes de ir al mapa
        if (earnedRewardRoll && DiceRollController.Instance != null)
        {
            if (IsCurrentWorldComplete())
            {
                DiceRollController.Instance.ShowFromLevelComplete(
                    DiceType.Normal,
                    () => worldCompleteController.Show(currentWorldData));
            }
            else
            {
                DiceRollController.Instance.Show(DiceType.Normal);
            }
            // El flujo continúa cuando DiceRollController llame SceneUtils.ToSelectionScene()
            // via su botón "Continuar". El LevelManager cede el control aquí.
            currentCoroutine = null;
            yield break;
        }

        if (IsCurrentWorldComplete())
        {
            worldCompleteController.Show(currentWorldData);
            currentCoroutine = null;
            yield break;
        }

        SceneUtils.ToSelectionScene();
    }

    private bool IsCurrentWorldComplete()
    {
        return currentWorldData != null && currentWorldData.LevelCount > 0 && worldCompleteController != null &&
               gameObject.scene.name == currentWorldData.GetLevelScene(currentWorldData.LevelCount - 1) &&
               WorldProgressManager.Instance != null &&
               WorldProgressManager.Instance.IsWorldComplete(currentWorldData);
    }

    private LevelRating SaveProgress()
    {
        int levelNumber = SceneUtils.Get_CurrentLevelName();

        // Siempre desbloquear el siguiente nivel
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UnlockNextLevel(levelNumber);
            if (levelNumber == 0)
                SaveManager.Instance.MarkTutorialCompleted();
        }

        LevelRating rating = LevelRating.OneStar;

        // Guardar gemas y tiempo solo en niveles reales (no el nivel 0 que es el tutorial)
        if (levelNumber > 0 && SaveManager.Instance != null)
        {
            // Compare against the previous record so the map only celebrates new badges.
            float parTime = currentWorldData != null ? currentWorldData.GetTargetTime(gameObject.scene.name) : 0f;
            bool earnedGemBadge = totalGemsInLevel > 0 && gemsCollected >= totalGemsInLevel &&
                                  SaveManager.Instance.LoadGems(levelNumber) < totalGemsInLevel;
            bool earnedTimeBadge = parTime > 0f && timeInLevel <= parTime &&
                                   SaveManager.Instance.LoadBestTime(levelNumber) > parTime;
            // The dice rewards progress, not repetition: the first clear and each badge earned
            // for the first time give one roll per run, however many arrive together.
            bool firstClear = !SaveManager.Instance.HasFinishedLevel(levelNumber);
            earnedRewardRoll = firstClear || earnedGemBadge || earnedTimeBadge || cloverFoundThisRun;
            MapPointsHolder.QueueBadgeArrival(levelNumber, earnedGemBadge, earnedTimeBadge);
            if (totalGemsInLevel > 0 && gemsCollected >= totalGemsInLevel)
                SaveManager.Instance.SetGemsPerfect(levelNumber);
            // A clear without taking any damage marks the level on the map.
            bool firstFlawless = IsFlawlessRun && !SaveManager.Instance.IsLevelFlawless(levelNumber);
            if (IsFlawlessRun)
                SaveManager.Instance.SetLevelFlawless(levelNumber);
            earnedRewardRoll |= firstFlawless;

            SaveManager.Instance.SaveGemsIfBetter(levelNumber, gemsCollected);
            SaveManager.Instance.SaveTimeIfBetter(levelNumber, timeInLevel);
        }

        // Calcular y guardar rating de estrellas
        if (levelNumber > 0)
        {
            var req = new LevelRatingSystem.LevelRequirements
            {
                targetTime = currentWorldData != null ? currentWorldData.GetTargetTime(gameObject.scene.name) : 0f,
                totalGems = totalGemsInLevel > 0 ? totalGemsInLevel : gemsCollected
            };
            rating = LevelRatingSystem.CalculateRating(timeInLevel, gemsCollected, req);
            SaveManager.Instance?.SaveRatingIfBetter(levelNumber, (int)rating);
        }

        UIController._instance?.ShowLevelRating(rating);
        Achievements.Evaluate();
        StoreReview.NotifyLevelCleared(levelNumber);

        // Analíticas
        if (levelNumber > 0 && AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.TrackLevelComplete(levelNumber, timeInLevel, gemsCollected);

        return rating;
    }
}
