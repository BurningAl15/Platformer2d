using UnityEngine;

public enum LevelRating
{
    None     = 0,
    OneStar  = 1,
    TwoStars = 2,
    ThreeStars = 3
}

/// <summary>
/// Calcula y persiste la calificación de estrellas de cada nivel.
///
/// CRITERIOS:
/// - 1 estrella: completar el nivel (siempre).
/// - 2 estrellas: recolectar todas las gemas del nivel.
/// - 3 estrellas: completar dentro del tiempo objetivo.
///
/// Los requisitos de gemas y tiempo por nivel se configuran en los
/// GameObjects MapPoint de la escena de selección (ya tienen totalGems y targetTime).
/// </summary>
public class LevelRatingSystem : MonoBehaviour
{
    public static LevelRatingSystem _instance;

    [System.Serializable]
    public class LevelRequirements
    {
        public float targetTime;
        public int   totalGems;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Cálculo
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Calcula la calificación basándose en el tiempo y gemas obtenidas.
    /// </summary>
    public LevelRating CalculateRating(float time, int gems, LevelRequirements req)
    {
        int stars = 1; // completar el nivel ya da 1 estrella

        if (req.totalGems > 0 && gems >= req.totalGems)
            stars++;

        if (req.targetTime > 0 && time <= req.targetTime)
            stars++;

        return (LevelRating)stars;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Utilidades de lectura (sin depender de SaveManager para ser portable)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna los sprites de estrellas llenas/vacías según el rating.
    /// totalSlots: cuántas estrellas mostrar en total (normalmente 3).
    /// </summary>
    public static bool[] GetStarStates(LevelRating rating, int totalSlots = 3)
    {
        bool[] states = new bool[totalSlots];
        int filled = (int)rating;
        for (int i = 0; i < totalSlots; i++)
            states[i] = i < filled;
        return states;
    }
}
