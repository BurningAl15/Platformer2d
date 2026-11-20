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
    private const string KEY_LIVES       = "PlayerLives";
    private const string KEY_HEART_FRAGMENT_COUNT = "PermanentHeartFragmentCount";
    private const string KEY_PERMANENT_HEART_COUNT = "PermanentHeartCount";
    private const int    DEFAULT_LIVES   = 3;
    // GDD: "sin límite máximo de vidas acumuladas"
    // Se mantiene un tope de seguridad alto (999) solo para evitar desbordamientos de PlayerPrefs.
    private const int    MAX_LIVES       = 999;
    private const string KEY_TUTORIAL_STARTED = "TutorialStarted";
    private const string KEY_TUTORIAL_COMPLETED = "TutorialCompleted";

    // Clave de IAP NoAds (gestionada por RevenueCatManager; solo se lee aquí)
    private const string KEY_NO_ADS = "IAP_NoAds_Purchased";

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

    public bool HasSaveData()
    {
        // A finished tutorial survives a progress reset, so Continue leads to the map.
        if (HasIncompleteTutorial() || IsTutorialCompleted())
            return true;

        // ResetProgress creates these keys with value 0. A key's existence does
        // not mean the player has unlocked or completed any level.
        for (int level = 1; level <= 10; level++)
        {
            if (PlayerPrefs.GetInt(StringUtils.Get_Level(level), 0) == 1)
                return true;
        }

        return false;
    }

    public bool HasIncompleteTutorial()
    {
        bool started = PlayerPrefs.GetInt(KEY_TUTORIAL_STARTED, 0) == 1;
        bool completed = PlayerPrefs.GetInt(KEY_TUTORIAL_COMPLETED, 0) == 1 ||
                         PlayerPrefs.GetInt(StringUtils.Get_Level(1), 0) == 1;
        return started && !completed;
    }

    public void MarkTutorialStarted()
    {
        PlayerPrefs.SetInt(KEY_TUTORIAL_STARTED, 1);
        PlayerPrefs.SetInt(KEY_TUTORIAL_COMPLETED, 0);
        PlayerPrefs.Save();
    }

    public void MarkTutorialCompleted()
    {
        PlayerPrefs.SetInt(KEY_TUTORIAL_STARTED, 1);
        PlayerPrefs.SetInt(KEY_TUTORIAL_COMPLETED, 1);
        PlayerPrefs.Save();
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1) return true;
        return PlayerPrefs.GetInt(StringUtils.Get_Level(levelNumber), 0) == 1;
    }

    public void UnlockNextLevel(int currentLevel)
    {
        int nextLevel = currentLevel + 1;
        PlayerPrefs.SetInt(StringUtils.Get_Level(nextLevel), 1);
        // The map list is zero-based and starts at Level_1 (index 0).
        PlayerPrefs.SetInt(StringUtils.playerPref_mapIndex, Mathf.Max(0, nextLevel - 1));
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
    {
        int stars = PlayerPrefs.GetInt($"Level_{levelNumber}_Rating", 0);
        // Older builds recorded completion times even when the rating component was absent.
        return stars == 0 && levelNumber > 0 && LoadBestTime(levelNumber) < 99999f ? 1 : stars;
    }

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
    //  Gems are a collectible, not a currency
    // ───────────────────────────────────────────────

    /// <summary>Distinct gems found: the best count of every level, so replays do not inflate it.</summary>
    public int CountCollectedGems(int totalLevels = 10)
    {
        int count = 0;
        for (int i = 1; i <= totalLevels; i++) count += LoadGems(i);
        return count;
    }

    // ───────────────────────────────────────────────
    //  Economía: Vidas
    // ───────────────────────────────────────────────

    /// <summary>Vidas disponibles del jugador. Mínimo 0, sin límite práctico (GDD).</summary>
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

    /// <summary>Agrega vidas (ej. desde un rewarded ad o dado de vidas). Sin límite máximo práctico.</summary>
    public void AddLives(int amount)
    {
        int lives = Mathf.Min(MAX_LIVES, LoadLives() + amount);
        SaveLives(lives);
    }

    public int GetMaxLives() => MAX_LIVES;
    public int GetDefaultLives() => DEFAULT_LIVES;

    // ───────────────────────────────────────────────
    //  Progresión: fragmentos de corazón
    // ───────────────────────────────────────────────

    /// <summary>True si el fragmento asignado a este nivel ya fue recogido.</summary>
    public bool HasHeartFragment(int levelNumber)
        => PlayerPrefs.GetInt($"HeartFragment_Level_{levelNumber}", 0) == 1;

    /// <summary>
    /// Registra como máximo un fragmento por nivel. Cada pareja otorga un corazón
    /// permanente. Retorna false si ese nivel ya había entregado su fragmento.
    /// </summary>
    public bool TryCollectHeartFragment(int levelNumber, out bool grantedPermanentHeart)
    {
        grantedPermanentHeart = false;
        if (levelNumber <= 0 || HasHeartFragment(levelNumber)) return false;

        PlayerPrefs.SetInt($"HeartFragment_Level_{levelNumber}", 1);
        int fragments = Mathf.Max(0, PlayerPrefs.GetInt(KEY_HEART_FRAGMENT_COUNT, 0)) + 1;
        PlayerPrefs.SetInt(KEY_HEART_FRAGMENT_COUNT, fragments);

        if (fragments % 2 == 0)
        {
            int hearts = Mathf.Max(0, PlayerPrefs.GetInt(KEY_PERMANENT_HEART_COUNT, 0)) + 1;
            PlayerPrefs.SetInt(KEY_PERMANENT_HEART_COUNT, hearts);
            grantedPermanentHeart = true;
        }

        PlayerPrefs.Save();
        return true;
    }

    public int LoadPermanentHeartCount()
        => Mathf.Max(0, PlayerPrefs.GetInt(KEY_PERMANENT_HEART_COUNT, 0));

    public int LoadHeartFragmentCount()
        => Mathf.Max(0, PlayerPrefs.GetInt(KEY_HEART_FRAGMENT_COUNT, 0));

    /// <summary>Fragmentos que faltan para el siguiente corazón permanente (0 o 1).</summary>
    public int LoadHeartFragmentsTowardNextHeart()
        => Mathf.Max(0, PlayerPrefs.GetInt(KEY_HEART_FRAGMENT_COUNT, 0)) % 2;

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
    /// <param name="keepTutorial">Keep a finished tutorial so the restart begins on the map.</param>
    public void ResetProgress(int totalLevels = 10, bool keepTutorial = false)
    {
        // Older saves mark the tutorial only by unlocking level 1.
        bool tutorialDone = keepTutorial && (IsTutorialCompleted() || PlayerPrefs.GetInt(StringUtils.Get_Level(1), 0) == 1);
        bool music = LoadMusicEnabled();
        bool sfx   = LoadSFXEnabled();
        var options = GameOptions.Snapshot();   // settings are not progress

        // Preservar el estado de NoAds — la compra IAP NO se borra al resetear el progreso
        int noAdsPurchased = PlayerPrefs.GetInt(KEY_NO_ADS, 0);

        PlayerPrefs.DeleteAll();

        SaveMusicEnabled(music);
        SaveSFXEnabled(sfx);

        // Restaurar NoAds después del borrado
        PlayerPrefs.SetInt(KEY_NO_ADS, noAdsPurchased);
        GameOptions.Restore(options);

        if (tutorialDone)
        {
            PlayerPrefs.SetInt(KEY_TUTORIAL_STARTED, 1);
            PlayerPrefs.SetInt(KEY_TUTORIAL_COMPLETED, 1);
        }

        // Vidas iniciales
        SaveLives(DEFAULT_LIVES);

        // Gemas globales en cero

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

    // ───────────────────────────────────────────────
    //  Player skins and their unlock progress
    // ───────────────────────────────────────────────

    private const string KEY_SKIN_STYLE = "PlayerSkin_Style";
    private const string KEY_SKIN_COLOR = "PlayerSkin_Color";

    public bool HasGemsPerfect(int levelNumber)
        => PlayerPrefs.GetInt($"GemsPerfect_Level_{levelNumber}", 0) == 1;

    /// <summary>Records that a level was finished with every gem collected.</summary>
    public void SetGemsPerfect(int levelNumber)
    {
        if (HasGemsPerfect(levelNumber)) return;
        PlayerPrefs.SetInt($"GemsPerfect_Level_{levelNumber}", 1);
        PlayerPrefs.Save();
    }

    public int CountPerfectGemLevels(int totalLevels = 10)
    {
        int count = 0;
        for (int i = 1; i <= totalLevels; i++)
            if (HasGemsPerfect(i)) count++;
        return count;
    }

    /// <summary>Levels whose clover was found. Spending clovers does not lower it.</summary>
    public int CountLevelClovers(int totalLevels = 10)
    {
        int count = 0;
        for (int i = 1; i <= totalLevels; i++)
            if (HasTrebol(i)) count++;
        return count;
    }

    public bool IsTutorialCompleted() => PlayerPrefs.GetInt(KEY_TUTORIAL_COMPLETED, 0) == 1;

    /// <summary>True once the level has been finished at least once.</summary>
    public bool HasFinishedLevel(int levelNumber) => LoadBestTime(levelNumber) < 99999f;

    /// <summary>Generic persistent on/off flag (achievements, notices already shown).</summary>
    // ── Amber shards (Amber Heart exploration) ──

    public bool HasAmberShard(int levelNumber, string shardId) => GetFlag($"Amber_{levelNumber}_{shardId}");

    /// <summary>Records a shard; returns false if it was already found.</summary>
    public bool CollectAmberShard(int levelNumber, string shardId)
    {
        if (HasAmberShard(levelNumber, shardId)) return false;
        PlayerPrefs.SetInt($"Amber_{levelNumber}_{shardId}", 1);
        PlayerPrefs.SetInt($"AmberCount_{levelNumber}", CountAmberShards(levelNumber) + 1);
        PlayerPrefs.Save();
        return true;
    }

    public int CountAmberShards(int levelNumber) => PlayerPrefs.GetInt($"AmberCount_{levelNumber}", 0);

    /// <summary>Shards placed in a level, recorded when the level is played.</summary>
    public void SaveAmberTotal(int levelNumber, int total)
    {
        if (PlayerPrefs.GetInt($"AmberTotal_{levelNumber}", -1) == total) return;
        PlayerPrefs.SetInt($"AmberTotal_{levelNumber}", total);
        PlayerPrefs.Save();
    }

    public int LoadAmberTotal(int levelNumber) => PlayerPrefs.GetInt($"AmberTotal_{levelNumber}", 0);

    public int CountAllAmberShards(int totalLevels = 10)
    {
        int count = 0;
        for (int i = 1; i <= totalLevels; i++) count += CountAmberShards(i);
        return count;
    }

    public int LoadAllAmberTotals(int totalLevels = 10)
    {
        int count = 0;
        for (int i = 1; i <= totalLevels; i++) count += LoadAmberTotal(i);
        return count;
    }

    /// <summary>A level finished at least once without taking any damage.</summary>
    public bool IsLevelFlawless(int levelNumber) => GetFlag("Flawless_Level_" + levelNumber);

    public void SetLevelFlawless(int levelNumber) => SetFlag("Flawless_Level_" + levelNumber);

    public int CountFlawlessLevels(int totalLevels = 10)
    {
        int count = 0;
        for (int i = 1; i <= totalLevels; i++)
            if (IsLevelFlawless(i)) count++;
        return count;
    }

    public bool GetFlag(string key) => PlayerPrefs.GetInt(key, 0) == 1;

    public void SetFlag(string key)
    {
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    public int LoadSkinStyle() => PlayerPrefs.GetInt(KEY_SKIN_STYLE, 0);
    public int LoadSkinColor() => PlayerPrefs.GetInt(KEY_SKIN_COLOR, 0);

    public void SaveSkinStyle(int index)
    {
        PlayerPrefs.SetInt(KEY_SKIN_STYLE, index);
        PlayerPrefs.Save();
    }

    public void SaveSkinColor(int index)
    {
        PlayerPrefs.SetInt(KEY_SKIN_COLOR, index);
        PlayerPrefs.Save();
    }

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
