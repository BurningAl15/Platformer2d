using System;
using UnityEngine;

/// <summary>
/// Gestiona las vidas globales del jugador a través de sesiones.
/// Las vidas persisten entre niveles (no se resetean al morir,
/// solo al quedarse sin vidas o al iniciar nueva partida).
///
/// FLUJO:
/// 1. Al morir (currentHealth = 0), PlayerHealth llama LevelManager.RespawnPlayer().
/// 2. LevelManager consulta LivesManager.ConsumeLifeAndCheck():
///    - Si quedan vidas → respawn normal.
///    - Si no quedan → disparar OnGameOver para mostrar pantalla de Game Over.
/// 3. GameOverController escucha OnGameOver y muestra las opciones.
///
/// ACCIONES DE RECUPERACIÓN:
/// - Rewarded ad → LivesManager.AddLives(2–3) → continuar.
/// - Gastar gemas  → LivesManager.AddLives(1) via SpendGemsForLife().
/// - Volver al mapa → resetear vidas y cargar selección de nivel.
/// </summary>
public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [Header("Configuración")]
    [Tooltip("Coste en gemas globales para comprar una vida extra.")]
    [SerializeField] private int gemCostPerLife = 10;
    [Tooltip("Vidas que da un rewarded ad.")]
    [SerializeField] private int livesPerAd = 2;

    /// <summary>Se dispara cuando el jugador se queda sin vidas.</summary>
    public event Action OnGameOver;
    /// <summary>Se dispara cuando las vidas cambian (para actualizar la UI).</summary>
    public event Action<int> OnLivesChanged;

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
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  API pública
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Vidas disponibles actualmente.</summary>
    public int CurrentLives => SaveManager.Instance != null
        ? SaveManager.Instance.LoadLives()
        : 3;

    /// <summary>
    /// Consume una vida. Si no quedan, dispara OnGameOver.
    /// Retorna true si el jugador puede continuar (quedan vidas).
    /// </summary>
    public bool ConsumeLifeAndCheck()
    {
        if (SaveManager.Instance == null) return true; // fallback: siempre continuar

        int remaining = SaveManager.Instance.ConsumeLife();
        OnLivesChanged?.Invoke(remaining);

        if (remaining <= 0)
        {
            OnGameOver?.Invoke();
            return false;
        }
        return true;
    }

    /// <summary>Añade vidas (ej. desde rewarded ad).</summary>
    public void AddLivesFromAd()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.AddLives(livesPerAd);
        OnLivesChanged?.Invoke(SaveManager.Instance.LoadLives());
    }

    /// <summary>
    /// Gasta gemas globales para obtener una vida extra.
    /// Retorna true si la transacción fue exitosa.
    /// </summary>
    public bool SpendGemsForLife()
    {
        if (SaveManager.Instance == null) return false;

        bool success = SaveManager.Instance.SpendGlobalGems(gemCostPerLife);
        if (success)
        {
            SaveManager.Instance.AddLives(1);
            OnLivesChanged?.Invoke(SaveManager.Instance.LoadLives());
        }
        return success;
    }

    /// <summary>
    /// ¿Puede el jugador comprar una vida con gemas?
    /// </summary>
    public bool CanAffordLife()
    {
        if (SaveManager.Instance == null) return false;
        return SaveManager.Instance.LoadGlobalGems() >= gemCostPerLife;
    }

    public int GetGemCostPerLife() => gemCostPerLife;
    public int GetLivesPerAd()     => livesPerAd;
}
