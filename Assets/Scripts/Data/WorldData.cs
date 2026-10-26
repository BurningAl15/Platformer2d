using UnityEngine;

/// <summary>
/// ScriptableObject que define un mundo del juego.
/// Un mundo agrupa un conjunto de niveles con nombre, condición de desbloqueo
/// y metadatos para el mapa de selección.
///
/// CREAR UN NUEVO MUNDO:
/// Assets → Create → Fox's Tale → World Data
///
/// CAMPOS:
/// - worldName: nombre del mundo (ej. "Mundo 1 — El Bosque")
/// - worldNumber: número del mundo (1, 2, 3...)
/// - levelSceneNames: nombres de escena Unity (ej. "Level_1", "Level_2"...)
/// - starsRequiredToUnlock: estrellas totales del mundo anterior necesarias para desbloquear
/// - totalGems: total de gemas en todo el mundo (para el resumen de mundo completado)
/// - totalTrebols: total de tréboles posibles en el mundo (= número de niveles)
/// - totalDiceFragments: fragmentos de dado secreto en el mundo (2-3 por mundo)
/// </summary>
[CreateAssetMenu(fileName = "WorldData_World1", menuName = "Fox's Tale/World Data")]
public class WorldData : ScriptableObject
{
    [Header("Identificación")]
    [Tooltip("Nombre mostrado al jugador (ej. 'Mundo 1 — El Bosque')")]
    public string worldName = "Mundo 1";

    [Tooltip("Número de mundo (1-indexed). Usado como clave de PlayerPrefs y para ordenar.")]
    public int worldNumber = 1;

    [Header("Niveles")]
    [Tooltip("Nombres de escena Unity en orden. Deben coincidir exactamente con los nombres en Build Settings.")]
    public string[] levelSceneNames = { "Level_0", "Level_1" };

    [Header("Desbloqueo")]
    [Tooltip("Estrellas totales del mundo ANTERIOR necesarias para desbloquear este mundo. 0 = siempre desbloqueado.")]
    public int starsRequiredToUnlock = 0;

    [Header("Coleccionables del mundo")]
    [Tooltip("Total de gemas en todo el mundo (suma de gemas de todos los niveles). Para estadísticas.")]
    public int totalGems = 0;

    [Tooltip("Total de tréboles posibles. Normalmente = número de niveles.")]
    public int totalTrebols = 0;

    [Tooltip("Fragmentos de dado secreto en este mundo (2-3 máximo según GDD).")]
    [Range(0, 3)]
    public int totalDiceFragments = 3;

    [Header("Presentación")]
    [Tooltip("Sprite o thumbnail del mundo para la pantalla de selección de mundos (futuro).")]
    public Sprite worldThumbnail;

    [Tooltip("Color de acento del mundo para UI.")]
    public Color worldColor = Color.white;

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Número de niveles en este mundo.</summary>
    public int LevelCount => levelSceneNames?.Length ?? 0;

    /// <summary>Retorna el nombre de escena del nivel por índice (0-based).</summary>
    public string GetLevelScene(int index)
    {
        if (levelSceneNames == null || index < 0 || index >= levelSceneNames.Length)
            return null;
        return levelSceneNames[index];
    }

    /// <summary>Máximo de estrellas posibles en este mundo (3 por nivel).</summary>
    public int MaxStars => LevelCount * 3;
}
