using System.Collections.Generic;
using UnityEngine;

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
        PositionPlayer();
    }

    private void ConfigureMapPoints()
    {
        bool hasSaveData = SaveManager.Instance != null
            ? SaveManager.Instance.HasSaveData()
            : PlayerPrefs.HasKey("Level_1");

        if (!hasSaveData) return;

        for (int i = 0; i < _mapPoints.Count; i++)
        {
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

            // Mejor tiempo
            _mapPoints[i].bestTime = SaveManager.Instance != null
                ? SaveManager.Instance.LoadBestTime(levelNumber)
                : PlayerPrefs.GetFloat(StringUtils.Get_TimeInLevel(levelNumber), 99999f);
            _mapPoints[i].CheckTime();

            // Trébol 🍀
            _mapPoints[i].hasTrebol = SaveManager.Instance != null
                ? SaveManager.Instance.HasTrebol(levelNumber)
                : PlayerPrefs.GetInt($"Trebol_Level_{levelNumber}", 0) == 1;
            _mapPoints[i].CheckTrebol();
        }
    }

    private void PositionPlayer()
    {
        int mapIndex = SaveManager.Instance != null
            ? SaveManager.Instance.LoadMapIndex()
            : Mathf.Max(0, PlayerPrefs.GetInt(StringUtils.playerPref_mapIndex, 0));

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
