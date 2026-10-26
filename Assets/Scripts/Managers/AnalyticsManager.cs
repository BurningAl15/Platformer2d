using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registra eventos de gameplay para análisis.
///
/// BACKEND: usa Unity Analytics (compilación condicional UNITY_ANALYTICS).
/// DEBUG:   loguea en consola cuando enableDebugLogs = true.
/// FALLBACK: ya NO guarda en PlayerPrefs para evitar acumulación indefinida.
///           Si se necesita persistencia local de eventos, implementar
///           un sistema de cola con límite de entradas.
/// </summary>
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private bool enableDebugLogs = true;

    // Estado de sesión
    private string sessionId;
    private DateTime sessionStartTime;

    // Estado del nivel actual
    private int currentLevelNumber;
    private int deathsInCurrentLevel;
    private int checkpointsReachedInCurrentLevel;
    private float levelStartTime;

    private Dictionary<string, object> sessionData = new Dictionary<string, object>();

    // ─────────────────────────────────────────────────────────────────────────
    //  Ciclo de vida
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeSession();
    }

    private void InitializeSession()
    {
        sessionId        = Guid.NewGuid().ToString();
        sessionStartTime = DateTime.Now;

        sessionData["session_id"]       = sessionId;
        sessionData["device_model"]     = SystemInfo.deviceModel;
        sessionData["device_type"]      = SystemInfo.deviceType.ToString();
        sessionData["operating_system"] = SystemInfo.operatingSystem;
        sessionData["game_version"]     = Application.version;
        sessionData["platform"]         = Application.platform.ToString();
        sessionData["is_mobile"]        = Application.isMobilePlatform;

        LogDebug("Session initialized", sessionData);
    }

    private void OnApplicationQuit()
    {
        if (currentLevelNumber > 0)
            TrackLevelAbandoned(currentLevelNumber, Time.time - levelStartTime);

        var data = new Dictionary<string, object>
        {
            { "session_duration_minutes", (DateTime.Now - sessionStartTime).TotalMinutes },
            { "session_id", sessionId },
            { "timestamp", Timestamp() }
        };
        LogDebug("Session ended", data);
        SendEvent("session_end", data);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Eventos de tracking
    // ─────────────────────────────────────────────────────────────────────────

    public void TrackLevelStart(int levelNumber)
    {
        currentLevelNumber              = levelNumber;
        deathsInCurrentLevel            = 0;
        checkpointsReachedInCurrentLevel = 0;
        levelStartTime                  = Time.time;

        var data = BuildData(levelNumber);
        LogDebug($"Level {levelNumber} started", data);
        SendEvent("level_start", data);
    }

    public void TrackLevelComplete(int levelNumber, float completionTime, int gemsCollected)
    {
        var data = BuildData(levelNumber);
        data["completion_time"] = completionTime;
        data["gems_collected"]  = gemsCollected;
        data["deaths"]          = deathsInCurrentLevel;
        data["checkpoints"]     = checkpointsReachedInCurrentLevel;

        LogDebug($"Level {levelNumber} completed in {completionTime:F1}s", data);
        SendEvent("level_complete", data);
        ResetLevelState();
    }

    public void TrackPlayerDeath(int levelNumber, Vector2 position, string cause)
    {
        deathsInCurrentLevel++;
        var data = BuildData(levelNumber);
        data["death_count"]  = deathsInCurrentLevel;
        data["position_x"]   = position.x;
        data["position_y"]   = position.y;
        data["cause"]        = cause;
        data["time_alive"]   = Time.time - levelStartTime;

        LogDebug($"Player death #{deathsInCurrentLevel} (cause: {cause})", data);
        SendEvent("player_death", data);
    }

    public void TrackCheckpointReached(int levelNumber, int checkpointIndex)
    {
        checkpointsReachedInCurrentLevel++;
        var data = BuildData(levelNumber);
        data["checkpoint_index"]   = checkpointIndex;
        data["time_to_checkpoint"] = Time.time - levelStartTime;

        LogDebug($"Checkpoint {checkpointIndex} reached", data);
        SendEvent("checkpoint_reached", data);
    }

    public void TrackGemCollected(int levelNumber, int totalGems)
    {
        var data = BuildData(levelNumber);
        data["total_gems"] = totalGems;

        LogDebug($"Gem collected (total: {totalGems})", data);
        SendEvent("gem_collected", data);
    }

    public void TrackEnemyKilled(int levelNumber, string enemyType)
    {
        var data = BuildData(levelNumber);
        data["enemy_type"] = enemyType;

        LogDebug($"Enemy killed: {enemyType}", data);
        SendEvent("enemy_killed", data);
    }

    public void TrackLevelAbandoned(int levelNumber, float timeSpent)
    {
        var data = BuildData(levelNumber);
        data["time_spent"]           = timeSpent;
        data["deaths_before_quit"]   = deathsInCurrentLevel;

        LogDebug($"Level {levelNumber} abandoned after {timeSpent:F1}s", data);
        SendEvent("level_abandoned", data);
        ResetLevelState();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Getters de estado
    // ─────────────────────────────────────────────────────────────────────────

    public int GetDeathsInCurrentLevel()             => deathsInCurrentLevel;
    public int GetCheckpointsReachedInCurrentLevel() => checkpointsReachedInCurrentLevel;

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers privados
    // ─────────────────────────────────────────────────────────────────────────

    private void SendEvent(string eventName, Dictionary<string, object> data)
    {
#if UNITY_ANALYTICS
        try
        {
            Unity.Services.Analytics.AnalyticsService.Instance.CustomData(eventName, data);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AnalyticsManager] No se pudo enviar evento '{eventName}': {e.Message}");
        }
#endif
        // Sin guardado en PlayerPrefs — eliminado para evitar acumulación ilimitada de claves.
    }

    private Dictionary<string, object> BuildData(int levelNumber)
    {
        return new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "session_id",   sessionId },
            { "timestamp",    Timestamp() }
        };
    }

    private void ResetLevelState()
    {
        currentLevelNumber              = 0;
        deathsInCurrentLevel            = 0;
        checkpointsReachedInCurrentLevel = 0;
        levelStartTime                  = 0f;
    }

    private string Timestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    private void LogDebug(string message, Dictionary<string, object> data)
    {
        if (!enableDebugLogs) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<color=cyan>[ANALYTICS]</color> {message}");
        sb.Append("{");
        foreach (var kvp in data)
            sb.Append($" {kvp.Key}: {kvp.Value},");
        sb.Append(" }");

        Debug.Log(sb.ToString());
    }
}
