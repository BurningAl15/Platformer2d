using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lee el progreso guardado (vía SaveManager) y configura el estado
/// de cada MapPoint al cargar la pantalla de selección de niveles.
///
/// MODOS:
/// - Modo legacy (worldData = null): usa la lista manual de _mapPoints con índices secuenciales.
/// - Modo WorldData: lee la definición de niveles desde el ScriptableObject WorldData.
///   Permite múltiples mundos sin tocar código.
///
/// Para el MVP actual (1 mundo), dejar worldData vacío y usar el modo legacy.
/// Para múltiples mundos, asignar el WorldData en el Inspector.
/// </summary>
public class MapPointsHolder : MonoBehaviour
{
    private static int pendingCloverLevel = -1;
    private static Vector2 pendingCloverViewportPosition;
    private static bool hasPendingCloverArrival;
    private static int pendingBadgeLevel = -1;
    private static bool pendingGemBadge;
    private static bool pendingTimeBadge;

    [Header("Modo legacy (1 mundo)")]
    [SerializeField] private List<MapPoint> _mapPoints = new List<MapPoint>();

    [Header("Modo multi-mundo (asignar WorldData del mundo actual)")]
    [Tooltip("Si se asigna, los datos de niveles se leen desde el ScriptableObject WorldData.")]
    [SerializeField] private WorldData worldData;

    [Header("Jugador")]
    [SerializeField] private LSPlayerController player;

    private void Start()
    {
        ConfigureMapPoints();
        // Rewards from the level just finished play first; the player waits for them.
        if (player != null)
            player.LockInput(PendingArrivalDuration());
        PositionPlayer();
        PlayPendingCloverArrival();
    }

    public static void QueueCloverArrival(int levelNumber, Vector2 viewportPosition)
    {
        pendingCloverLevel = levelNumber;
        pendingCloverViewportPosition = viewportPosition;
        hasPendingCloverArrival = true;

        SceneManager.sceneLoaded -= ClearCloverArrivalOnOtherScene;
        SceneManager.sceneLoaded += ClearCloverArrivalOnOtherScene;
    }

    /// <summary>Queues the map arrival effect for badges earned by finishing a level.</summary>
    public static void QueueBadgeArrival(int levelNumber, bool gemBadge, bool timeBadge)
    {
        if (!gemBadge && !timeBadge)
            return;

        pendingBadgeLevel = levelNumber;
        pendingGemBadge = gemBadge;
        pendingTimeBadge = timeBadge;

        SceneManager.sceneLoaded -= ClearCloverArrivalOnOtherScene;
        SceneManager.sceneLoaded += ClearCloverArrivalOnOtherScene;
    }

    private static void ClearCloverArrivalOnOtherScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LevelSelection")
            return;

        ClearPendingCloverArrival();
    }

    private static void ClearPendingCloverArrival()
    {
        pendingCloverLevel = -1;
        hasPendingCloverArrival = false;
        pendingBadgeLevel = -1;
        pendingGemBadge = false;
        pendingTimeBadge = false;
        SceneManager.sceneLoaded -= ClearCloverArrivalOnOtherScene;
    }

    private const float ArrivalDuration = 0.8f;   // flight 0.48 s + reveal pulse 0.32 s

    /// <summary>Seconds until the queued badge and clover arrivals finish, plus a short beat.</summary>
    private float PendingArrivalDuration()
    {
        float end = 0f;
        if (hasPendingCloverArrival) end = ArrivalDuration;
        if (pendingBadgeLevel >= 0)
        {
            float delay = 0.25f;
            if (pendingGemBadge) { end = Mathf.Max(end, delay + ArrivalDuration); delay += 0.3f; }
            if (pendingTimeBadge) end = Mathf.Max(end, delay + ArrivalDuration);
        }
        return end > 0f ? end + 0.2f : 0f;
    }

    private void PlayPendingCloverArrival()
    {
        // Level-completion badges enter from the screen center, one after another.
        Vector2 screenCenter = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < _mapPoints.Count; i++)
        {
            if (_mapPoints[i] == null)
                continue;

            int levelNumber = GetLevelNumber(i);
            if (hasPendingCloverArrival && levelNumber == pendingCloverLevel)
                _mapPoints[i].AnimateCloverArrival(pendingCloverViewportPosition);

            if (levelNumber != pendingBadgeLevel)
                continue;

            float delay = 0.25f;
            if (pendingGemBadge && _mapPoints[i].AnimateGemArrival(screenCenter, delay))
                delay += 0.3f;
            if (pendingTimeBadge)
                _mapPoints[i].AnimateTimeArrival(screenCenter, delay);
        }

        ClearPendingCloverArrival();
    }

    private void ConfigureMapPoints()
    {
        for (int i = 0; i < _mapPoints.Count; i++)
        {
            if (_mapPoints[i] == null) continue;

            // Obtener el número de nivel: desde WorldData si está asignado, o secuencial
            int levelNumber = GetLevelNumber(i);
            if (levelNumber < 0) continue;

            // Estado de bloqueo (el primer nivel siempre está desbloqueado)
            if (i > 0)
            {
                _mapPoints[i].isLocked = SaveManager.Instance != null
                    ? !SaveManager.Instance.IsLevelUnlocked(levelNumber)
                    : PlayerPrefs.GetInt(StringUtils.Get_Level(levelNumber), 0) != 1;
            }

            // Gemas recolectadas
            _mapPoints[i].gemsCollected = SaveManager.Instance != null
                ? SaveManager.Instance.LoadGems(levelNumber)
                : PlayerPrefs.GetInt(StringUtils.Get_GemsInLevel(levelNumber), 0);
            _mapPoints[i].CheckGems();
            // Saves made before perfect runs were recorded still count toward skin unlocks.
            if (SaveManager.Instance != null && _mapPoints[i].totalGems > 0 &&
                _mapPoints[i].gemsCollected >= _mapPoints[i].totalGems)
                SaveManager.Instance.SetGemsPerfect(levelNumber);

            // Mejor tiempo
            _mapPoints[i].bestTime = SaveManager.Instance != null
                ? SaveManager.Instance.LoadBestTime(levelNumber)
                : PlayerPrefs.GetFloat(StringUtils.Get_TimeInLevel(levelNumber), 99999f);
            float parTime = worldData != null ? worldData.GetTargetTime(worldData.GetLevelScene(i)) : 0f;
            if (parTime > 0f) _mapPoints[i].targetTime = parTime;
            _mapPoints[i].CheckTime();

            // Trébol 🍀
            _mapPoints[i].hasTrebol = SaveManager.Instance != null
                ? SaveManager.Instance.HasTrebol(levelNumber)
                : PlayerPrefs.GetInt($"Trebol_Level_{levelNumber}", 0) == 1;
            _mapPoints[i].CheckTrebol();

            _mapPoints[i].hasHeartFragment = SaveManager.Instance != null
                ? SaveManager.Instance.HasHeartFragment(levelNumber)
                : PlayerPrefs.GetInt($"HeartFragment_Level_{levelNumber}", 0) == 1;
        }
    }

    private void PositionPlayer()
    {
        if (player == null || _mapPoints == null || _mapPoints.Count == 0)
        {
            Debug.LogError("[MapPointsHolder] Cannot position the selector: player or map points are missing.");
            return;
        }

        int mapIndex = SaveManager.Instance != null
            ? SaveManager.Instance.LoadMapIndex()
            : Mathf.Max(0, PlayerPrefs.GetInt(StringUtils.playerPref_mapIndex, 0));

        // Reconcile older builds that persisted an index one point behind the
        // newly unlocked level. The map list is ordered Level_1, Level_2, ... .
        for (int i = 0; i < _mapPoints.Count; i++)
        {
            MapPoint point = _mapPoints[i];
            if (point != null && point.isLevel && !point.isLocked)
                mapIndex = Mathf.Max(mapIndex, i);
        }

        mapIndex = Mathf.Clamp(mapIndex, 0, _mapPoints.Count - 1);
        player.SetCurrentPoint(_mapPoints[mapIndex]);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helper — número de nivel por índice
    // ─────────────────────────────────────────────────────────────────────────

    private int GetLevelNumber(int index)
    {
        if (worldData != null && index < worldData.LevelCount)
        {
            // Extraer número del nombre de escena (ej. "Level_3" → 3)
            string sceneName = worldData.GetLevelScene(index);
            if (!string.IsNullOrEmpty(sceneName))
            {
                string[] parts = sceneName.Split('_');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int n))
                    return n;
            }
            return -1;
        }

        // Modo legacy: índice + 1 (niveles 1, 2, 3...)
        return index + 1;
    }
}
