using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lee el progreso guardado (vía SaveManager) y configura el estado
/// de cada MapPoint al cargar la pantalla de selección de niveles.
/// </summary>
public class MapPointsHolder : MonoBehaviour
{
    [SerializeField] private List<MapPoint> _mapPoints = new List<MapPoint>();
    [SerializeField] private LSPlayerController player;
    [SerializeField] private int worldNumber;

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
            int levelNumber = i + 1;

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
}
