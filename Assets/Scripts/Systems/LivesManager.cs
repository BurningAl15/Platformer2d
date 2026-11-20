using System;
using UnityEngine;

/// <summary>
/// Gestiona las vidas globales del jugador a través de sesiones.
/// Administra las vidas globales de reintento, separadas de los corazones
/// de salud de PlayerHealth. Se consume una vida global cuando la salud llega a 0.
///
/// FLUJO:
/// 1. Al morir (salud = 0), PlayerHealth llama LevelManager.RespawnPlayer().
/// 2. LevelManager consulta LivesManager.ConsumeLifeAndCheck():
///    - Si quedan vidas → respawn normal.
///    - Si no quedan → disparar OnGameOver para mostrar pantalla de Game Over.
/// 3. GameOverController escucha OnGameOver y muestra las opciones.
///
/// ACCIONES DE RECUPERACIÓN:
/// - Rewarded ad → LivesManager.AddLives(2–3) → continuar.
/// - Volver al mapa → resetear vidas y cargar selección de nivel.
/// </summary>
public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [Header("Configuración")]
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

    public int GetLivesPerAd()     => livesPerAd;

    /// <summary>
    /// Dispara OnLivesChanged manualmente (ej. después de que SaveManager agrega vidas
    /// desde el dado o desde un ad).
    /// </summary>
    public void NotifyLivesChanged()
    {
        int lives = SaveManager.Instance != null ? SaveManager.Instance.LoadLives() : 0;
        OnLivesChanged?.Invoke(lives);
    }
}
