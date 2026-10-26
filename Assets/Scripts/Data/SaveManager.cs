using UnityEngine;

/// <summary>
/// Capa centralizada de persistencia del juego.
/// Todos los accesos a PlayerPrefs pasan por aquí.
/// Ninguna otra clase debería llamar PlayerPrefs directamente.
///
/// DATOS PERSISTIDOS:
/// - Progreso de niveles (desbloqueados, gemas por nivel, tiempo)
/// - Índice de posición en el mapa de selección
/// - Configuración de audio (música y SFX on/off)
/// - Economía: gemas globales (wallet), vidas
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Claves de PlayerPrefs para economía
    private const string KEY_GLOBAL_GEMS = "GlobalGems";
    private const string KEY_LIVES       = "PlayerLives";
    private const int    DEFAULT_LIVES   = 3;
    private const int    MAX_LIVES       = 9;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ───────────────────────────────────────────────
    //  Progreso de niveles
    // ───────────────────────────────────────────────

    public bool HasSaveData() => PlayerPrefs.HasKey(StringUtils.Get_Level(1));

    public bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1) return true;
        return PlayerPrefs.GetInt(StringUtils.Get_Level(levelNumber), 0) == 1;
    }

    public void UnlockNextLevel(int currentLevel)
    {
        int nextLevel = currentLevel + 1;
        PlayerPrefs.SetInt(StringUtils.Get_Level(nextLevel), 1);
        PlayerPrefs.SetInt(StringUtils.playerPref_mapIndex, nextLevel - 2);
        PlayerPrefs.Save();
    }

    public void SaveGemsIfBetter(int levelNumber, int gems)
    {
        string key = StringUtils.Get_GemsInLevel(levelNumber);
        int saved = PlayerPrefs.GetInt(key, 0);
        if (gems > saved)
        {
            PlayerPrefs.SetInt(key, gems);
            PlayerPrefs.Save();
        }
    }

    public void SaveTimeIfBetter(int levelNumber, float time)
    {
        string key = StringUtils.Get_TimeInLevel(levelNumber);
        float saved = PlayerPrefs.GetFloat(key, 99999f);
        if (time < saved)
        {
            PlayerPrefs.SetFloat(key, time);
            PlayerPrefs.Save();
        }
    }

    public int LoadGems(int levelNumber)
        => PlayerPrefs.GetInt(StringUtils.Get_GemsInLevel(levelNumber), 0);

    public float LoadBestTime(int levelNumber)
        => PlayerPrefs.GetFloat(StringUtils.Get_TimeInLevel(levelNumber), 99999f);

    public int LoadMapIndex()
        => Mathf.Max(0, PlayerPrefs.GetInt(StringUtils.playerPref_mapIndex, 0));

    // ───────────────────────────────────────────────
    //  Estrellas / Rating
    // ───────────────────────────────────────────────

    public int LoadRating(int levelNumber)
        => PlayerPrefs.GetInt($"Level_{levelNumber}_Rating", 0);

    /// <summary>Guarda el rating solo si es mejor que el anterior.</summary>
    public void SaveRatingIfBetter(int levelNumber, int stars)
    {
        int saved = LoadRating(levelNumber);
        if (stars > saved)
        {
            PlayerPrefs.SetInt($"Level_{levelNumber}_Rating", stars);
            PlayerPrefs.Save();
        }
    }

    // ───────────────────────────────────────────────
    //  Economía: Gemas globales (wallet)
    // ───────────────────────────────────────────────
    /// <summary>Total de gemas acumuladas a lo largo de toda la partida.</summary>
    public int LoadGlobalGems()
        => PlayerPrefs.GetInt(KEY_GLOBAL_GEMS, 0);

    /// <summary>Añade gemas al wallet global.</summary>
    public void AddGlobalGems(int amount)
    {
        int current = LoadGlobalGems();
        PlayerPrefs.SetInt(KEY_GLOBAL_GEMS, current + amount);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Gasta gemas del wallet si hay suficientes. Retorna true si la transacción fue exitosa.
    /// </summary>
    public bool SpendGlobalGems(int amount)
    {
        int current = LoadGlobalGems();
        if (current < amount) return false;
        PlayerPrefs.SetInt(KEY_GLOBAL_GEMS, current - amount);
        PlayerPrefs.Save();
        return true;
    }

    // ───────────────────────────────────────────────
    //  Economía: Vidas
    // ───────────────────────────────────────────────

    /// <summary>Vidas disponibles del jugador. Siempre entre 0 y MAX_LIVES.</summary>
    public int LoadLives()
        => PlayerPrefs.GetInt(KEY_LIVES, DEFAULT_LIVES);

    /// <summary>Guarda el número de vidas.</summary>
    public void SaveLives(int lives)
    {
        PlayerPrefs.SetInt(KEY_LIVES, Mathf.Clamp(lives, 0, MAX_LIVES));
        PlayerPrefs.Save();
    }

    /// <summary>Consume una vida. Retorna las vidas restantes.</summary>
    public int ConsumeLife()
    {
        int lives = Mathf.Max(0, LoadLives() - 1);
        SaveLives(lives);
        return lives;
    }

    /// <summary>Agrega vidas (ej. desde un rewarded ad). No supera MAX_LIVES.</summary>
    public void AddLives(int amount)
    {
        int lives = Mathf.Min(MAX_LIVES, LoadLives() + amount);
        SaveLives(lives);
    }

    public int GetMaxLives() => MAX_LIVES;
    public int GetDefaultLives() => DEFAULT_LIVES;

    // ───────────────────────────────────────────────
    //  Configuración de audio
    // ───────────────────────────────────────────────

    public bool LoadMusicEnabled() => PlayerPrefs.GetInt("MusicLevel", 1) == 1;
    public bool LoadSFXEnabled()   => PlayerPrefs.GetInt("SFXLevel", 1) == 1;

    public void SaveMusicEnabled(bool enabled)
    {
        PlayerPrefs.SetInt("MusicLevel", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SaveSFXEnabled(bool enabled)
    {
        PlayerPrefs.SetInt("SFXLevel", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ───────────────────────────────────────────────
    //  Nueva partida
    // ───────────────────────────────────────────────

    /// <summary>
    /// Borra todo el progreso. Preserva audio config. Reinicia vidas y gemas globales.
    /// </summary>
    public void ResetProgress(int totalLevels = 10)
    {
        bool music = LoadMusicEnabled();
        bool sfx   = LoadSFXEnabled();

        PlayerPrefs.DeleteAll();

        SaveMusicEnabled(music);
        SaveSFXEnabled(sfx);

        // Vidas iniciales
        SaveLives(DEFAULT_LIVES);

        // Gemas globales en cero
        PlayerPrefs.SetInt(KEY_GLOBAL_GEMS, 0);

        // Progreso de niveles
        for (int i = 1; i <= totalLevels; i++)
        {
            PlayerPrefs.SetInt(StringUtils.Get_Level(i), 0);
            PlayerPrefs.SetInt(StringUtils.Get_GemsInLevel(i), 0);
            PlayerPrefs.SetFloat(StringUtils.Get_TimeInLevel(i), 99999f);
        }

        PlayerPrefs.Save();
    }

    // ───────────────────────────────────────────────
    //  Tréboles 🍀 y Fragmentos de Dado Secreto 🎲
    // ───────────────────────────────────────────────

    private const string KEY_TOTAL_TREBOLS = "TotalTrebols";

    public bool HasTrebol(int levelNumber)
        => PlayerPrefs.GetInt($"Trebol_Level_{levelNumber}", 0) == 1;

    public void SetTrebolCollected(int levelNumber)
    {
        if (HasTrebol(levelNumber)) return;
        PlayerPrefs.SetInt($"Trebol_Level_{levelNumber}", 1);
        AddTrebols(1);
        PlayerPrefs.Save();
    }

    public int LoadTotalTrebols()
        => PlayerPrefs.GetInt(KEY_TOTAL_TREBOLS, 0);

    public void AddTrebols(int amount)
    {
        int current = LoadTotalTrebols();
        PlayerPrefs.SetInt(KEY_TOTAL_TREBOLS, current + amount);
        PlayerPrefs.Save();
    }

    public bool SpendTrebol()
    {
        int current = LoadTotalTrebols();
        if (current <= 0) return false;
        PlayerPrefs.SetInt(KEY_TOTAL_TREBOLS, current - 1);
        PlayerPrefs.Save();
        return true;
    }

    public bool HasDiceFragment(int worldNumber, int levelNumber)
        => PlayerPrefs.GetInt($"DiceFrag_W{worldNumber}_L{levelNumber}", 0) == 1;

    public void SetDiceFragmentCollected(int worldNumber, int levelNumber)
    {
        if (HasDiceFragment(worldNumber, levelNumber)) return;
        PlayerPrefs.SetInt($"DiceFrag_W{worldNumber}_L{levelNumber}", 1);
        int currentCount = GetDiceFragmentsCount(worldNumber);
        PlayerPrefs.SetInt($"DiceFrag_Count_W{worldNumber}", currentCount + 1);
        PlayerPrefs.Save();
    }

    public int GetDiceFragmentsCount(int worldNumber)
        => PlayerPrefs.GetInt($"DiceFrag_Count_W{worldNumber}", 0);

    public bool IsSecretDiceUnlocked(int worldNumber, int requiredFragments = 3)
        => GetDiceFragmentsCount(worldNumber) >= requiredFragments;
}
