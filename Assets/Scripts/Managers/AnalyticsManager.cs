using System;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [Header("Session Data")]
    [SerializeField] private string sessionId;
    [SerializeField] private DateTime sessionStartTime;
    
    [Header("Current Level Data")]
    [SerializeField] private int currentLevelNumber;
    [SerializeField] private int deathsInCurrentLevel;
    [SerializeField] private int checkpointsReachedInCurrentLevel;
    [SerializeField] private float levelStartTime;
    
    [Header("Debug Mode")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private Dictionary<string, object> sessionData = new Dictionary<string, object>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSession();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSession()
    {
        sessionId = System.Guid.NewGuid().ToString();
        sessionStartTime = DateTime.Now;
        
        sessionData["session_id"] = sessionId;
        sessionData["device_model"] = SystemInfo.deviceModel;
        sessionData["device_type"] = SystemInfo.deviceType.ToString();
        sessionData["operating_system"] = SystemInfo.operatingSystem;
        sessionData["unity_version"] = Application.unityVersion;
        sessionData["game_version"] = Application.version;
        sessionData["platform"] = Application.platform.ToString();
        sessionData["is_mobile"] = Application.isMobilePlatform;
        
        LogDebug("Analytics Session Initialized", sessionData);
    }

    public void TrackLevelStart(int levelNumber)
    {
        currentLevelNumber = levelNumber;
        deathsInCurrentLevel = 0;
        checkpointsReachedInCurrentLevel = 0;
        levelStartTime = Time.time;
        
        var eventData = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "session_id", sessionId }
        };
        
        LogDebug($"Level {levelNumber} Started", eventData);
        SendAnalyticsEvent("level_start", eventData);
    }

    public void TrackLevelComplete(int levelNumber, float completionTime, int gemsCollected)
    {
        var eventData = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "completion_time", completionTime },
            { "gems_collected", gemsCollected },
            { "deaths", deathsInCurrentLevel },
            { "checkpoints_used", checkpointsReachedInCurrentLevel },
            { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "session_id", sessionId }
        };
        
        LogDebug($"Level {levelNumber} Completed", eventData);
        SendAnalyticsEvent("level_complete", eventData);
        
        ResetLevelStats();
    }

    public void TrackPlayerDeath(int levelNumber, Vector2 position, string cause)
    {
        deathsInCurrentLevel++;
        
        var eventData = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "death_count", deathsInCurrentLevel },
            { "position_x", position.x },
            { "position_y", position.y },
            { "cause_of_death", cause },
            { "time_alive", Time.time - levelStartTime },
            { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "session_id", sessionId }
        };
        
        LogDebug($"Player Death #{deathsInCurrentLevel} in Level {levelNumber}", eventData);
        SendAnalyticsEvent("player_death", eventData);
    }

    public void TrackCheckpointReached(int levelNumber, int checkpointIndex)
    {
        checkpointsReachedInCurrentLevel++;
        
        var eventData = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "checkpoint_index", checkpointIndex },
            { "time_to_checkpoint", Time.time - levelStartTime },
            { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "session_id", sessionId }
        };
        
        LogDebug($"Checkpoint {checkpointIndex} Reached in Level {levelNumber}", eventData);
        SendAnalyticsEvent("checkpoint_reached", eventData);
    }

    public void TrackGemCollected(int levelNumber, int totalGemsCollected)
    {
        var eventData = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "gems_collected", totalGemsCollected },
            { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "session_id", sessionId }
        };
        
        LogDebug($"Gem Collected (Total: {totalGemsCollected})", eventData);
        SendAnalyticsEvent("gem_collected", eventData);
    }

    public void TrackEnemyKilled(int levelNumber, string enemyType)
    {
        var eventData = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "enemy_type", enemyType },
            { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "session_id", sessionId }
        };
        
        LogDebug($"Enemy Killed: {enemyType}", eventData);
        SendAnalyticsEvent("enemy_killed", eventData);
    }

    public void TrackLevelAbandoned(int levelNumber, float timeSpent)
    {
        var eventData = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "time_spent", timeSpent },
            { "deaths_before_quit", deathsInCurrentLevel },
            { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "session_id", sessionId }
        };
        
        LogDebug($"Level {levelNumber} Abandoned", eventData);
        SendAnalyticsEvent("level_abandoned", eventData);
        
        ResetLevelStats();
    }

    private void SendAnalyticsEvent(string eventName, Dictionary<string, object> eventData)
    {
        #if UNITY_ANALYTICS || UNITY_CLOUD_BUILD
        try
        {
            Analytics.CustomEvent(eventName, eventData);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Analytics event '{eventName}' could not be sent: {e.Message}");
        }
        #endif
        
        SaveAnalyticsToPlayerPrefs(eventName, eventData);
    }

    private void SaveAnalyticsToPlayerPrefs(string eventName, Dictionary<string, object> eventData)
    {
        string key = $"Analytics_{eventName}_{DateTime.Now:yyyyMMdd_HHmmss}";
        string jsonData = JsonUtility.ToJson(new AnalyticsEventWrapper(eventName, eventData));
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.Save();
    }

    private void LogDebug(string message, Dictionary<string, object> data)
    {
        if (!enableDebugLogs) return;
        
        string dataString = "{\n";
        foreach (var kvp in data)
        {
            dataString += $"  {kvp.Key}: {kvp.Value}\n";
        }
        dataString += "}";
        
        Debug.Log($"<color=cyan>[ANALYTICS]</color> {message}\n{dataString}");
    }

    private void ResetLevelStats()
    {
        currentLevelNumber = 0;
        deathsInCurrentLevel = 0;
        checkpointsReachedInCurrentLevel = 0;
        levelStartTime = 0f;
    }

    public int GetDeathsInCurrentLevel()
    {
        return deathsInCurrentLevel;
    }

    public int GetCheckpointsReachedInCurrentLevel()
    {
        return checkpointsReachedInCurrentLevel;
    }

    private void OnApplicationQuit()
    {
        if (currentLevelNumber > 0)
        {
            TrackLevelAbandoned(currentLevelNumber, Time.time - levelStartTime);
        }
        
        var sessionEndData = new Dictionary<string, object>
        {
            { "session_duration", (DateTime.Now - sessionStartTime).TotalMinutes },
            { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "session_id", sessionId }
        };
        
        LogDebug("Session Ended", sessionEndData);
        SendAnalyticsEvent("session_end", sessionEndData);
    }
}

[Serializable]
public class AnalyticsEventWrapper
{
    public string eventName;
    public string eventDataJson;
    
    public AnalyticsEventWrapper(string name, Dictionary<string, object> data)
    {
        eventName = name;
        eventDataJson = JsonUtility.ToJson(data);
    }
}
