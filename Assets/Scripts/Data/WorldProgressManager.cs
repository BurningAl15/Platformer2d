using UnityEngine;

/// <summary>
/// Gestiona el progreso por mundo: estrellas totales, desbloqueo del siguiente mundo
/// y verificación de si un mundo está completado.
///
/// Trabaja en conjunto con SaveManager (por nivel) y WorldData (definición de mundos).
/// </summary>
public class WorldProgressManager : MonoBehaviour
{
    public static WorldProgressManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Consultas de progreso por mundo
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Suma de estrellas obtenidas en todos los niveles del mundo.
    /// </summary>
    public int GetTotalStarsInWorld(WorldData world)
    {
        if (world == null || SaveManager.Instance == null) return 0;

        int total = 0;
        foreach (string sceneName in world.levelSceneNames)
        {
            int levelNumber = ParseLevelNumber(sceneName);
            if (levelNumber >= 0)
                total += SaveManager.Instance.LoadRating(levelNumber);
        }
        return total;
    }

    /// <summary>
    /// Retorna true si el jugador puede desbloquear el mundo indicado.
    /// Comprueba las estrellas del mundo anterior.
    /// </summary>
    public bool IsWorldUnlocked(WorldData world, WorldData previousWorld)
    {
        if (world == null) return false;
        if (world.starsRequiredToUnlock <= 0) return true;    // Primer mundo
        if (previousWorld == null) return false;

        int stars = GetTotalStarsInWorld(previousWorld);
        return stars >= world.starsRequiredToUnlock;
    }

    /// <summary>
    /// Retorna true si todos los niveles del mundo están completados (al menos 1 estrella).
    /// </summary>
    public bool IsWorldComplete(WorldData world)
    {
        if (world == null || SaveManager.Instance == null) return false;

        foreach (string sceneName in world.levelSceneNames)
        {
            int levelNumber = ParseLevelNumber(sceneName);
            if (levelNumber < 0) continue;
            if (SaveManager.Instance.LoadRating(levelNumber) == 0)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Gemas totales recolectadas en el mundo (mejor run por nivel).
    /// </summary>
    public int GetGemsCollectedInWorld(WorldData world)
    {
        if (world == null || SaveManager.Instance == null) return 0;

        int total = 0;
        foreach (string sceneName in world.levelSceneNames)
        {
            int levelNumber = ParseLevelNumber(sceneName);
            if (levelNumber >= 0)
                total += SaveManager.Instance.LoadGems(levelNumber);
        }
        return total;
    }

    /// <summary>
    /// Tréboles recolectados en el mundo.
    /// </summary>
    public int GetTrebolsInWorld(WorldData world)
    {
        if (world == null || SaveManager.Instance == null) return 0;

        int total = 0;
        foreach (string sceneName in world.levelSceneNames)
        {
            int levelNumber = ParseLevelNumber(sceneName);
            if (levelNumber >= 0 && SaveManager.Instance.HasTrebol(levelNumber))
                total++;
        }
        return total;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helper privado
    // ─────────────────────────────────────────────────────────────────────────

    private int ParseLevelNumber(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return -1;
        string[] parts = sceneName.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out int number))
            return number;
        return -1;
    }
}
