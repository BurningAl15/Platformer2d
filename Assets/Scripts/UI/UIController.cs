using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
//  Clase auxiliar para los corazones de salud
// ─────────────────────────────────────────────────────────────────────────────
[Serializable]
public class HeartUI
{
    public Image heartImage;
    private Sprite heartEmpty;
    private Sprite heartHalf;
    private Sprite heartFull;

    public void Init(Sprite empty, Sprite half, Sprite full)
    {
        heartEmpty = empty;
        heartHalf  = half;
        heartFull  = full;
        heartImage.sprite = heartFull;
    }

    public void UpdateHeartSprite_Damage(int currentHealth)
    {
        heartImage.sprite = currentHealth % 2 == 0 ? heartEmpty : heartHalf;
    }

    public void UpdateHeartSprite_Cure(int currentHealth)
    {
        heartImage.sprite = currentHealth % 2 == 1 ? heartFull : heartHalf;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
//  UIController — HUD de gameplay
// ─────────────────────────────────────────────────────────────────────────────
/// <summary>
/// Controla todos los elementos del HUD durante el gameplay:
/// - Corazones de salud.
/// - Contador de gemas del nivel y wallet global.
/// - Animaciones de fin de nivel.
/// - Pantalla de estrellas al completar el nivel.
/// </summary>
public class UIController : MonoBehaviour
{
    public static UIController _instance;

    // ─────────────────────────────
    //  Salud
    // ─────────────────────────────
    [Header("Corazones")]
    [SerializeField] private List<HeartUI> hearts = new List<HeartUI>();
    [SerializeField] private Sprite heartEmpty;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartFull;

    // ─────────────────────────────
    //  Gemas
    // ─────────────────────────────
    [Header("Gemas")]
    [SerializeField] private TextMeshProUGUI gemsLevelText;
    [Tooltip("(Opcional) Texto del wallet global de gemas en el HUD")]
    [SerializeField] private TextMeshProUGUI gemsGlobalText;

    // ─────────────────────────────
    //  Tréboles 🍀
    // ─────────────────────────────
    [Header("Tréboles")]
    [Tooltip("(Opcional) Texto o contador de tréboles acumulados en el HUD")]
    [SerializeField] private TextMeshProUGUI trebolCountText;

    // ─────────────────────────────
    //  Vidas
    // ─────────────────────────────
    [Header("Vidas")]
    [Tooltip("(Opcional) Texto o imagen que muestra las vidas restantes en el HUD")]
    [SerializeField] private TextMeshProUGUI livesText;

    // ─────────────────────────────
    //  Fin de nivel
    // ─────────────────────────────
    [Header("Fin de nivel")]
    [SerializeField] private Animator endLevel_animator;
    [SerializeField] private Animator amazingJob_animator;

    [Header("Estrellas de calificación")]
    [Tooltip("Imágenes de las 3 estrellas en la pantalla de fin de nivel")]
    [SerializeField] private List<Image> starImages;
    [SerializeField] private Sprite starFull;
    [SerializeField] private Sprite starEmpty;

    private Coroutine currentCoroutine = null;

    // ─────────────────────────────────────────────────────────────────────────
    //  Ciclo de vida
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;

        foreach (var heart in hearts)
            heart.Init(heartEmpty, heartHalf, heartFull);

        if (endLevel_animator != null) endLevel_animator.gameObject.SetActive(false);
        if (amazingJob_animator != null) amazingJob_animator.gameObject.SetActive(false);
    }

    private void Start()
    {
        FadeEffect._instance.Initialize();

        if (endLevel_animator != null) endLevel_animator.gameObject.SetActive(false);
        if (amazingJob_animator != null) amazingJob_animator.gameObject.SetActive(false);

        // Mostrar vidas iniciales
        RefreshLivesUI();

        // Mostrar gemas globales si hay texto asignado
        RefreshGlobalGemsUI();

        // Mostrar tréboles acumulados
        RefreshTrebolUI();

        // Suscribirse a cambios de vidas
        if (LivesManager.Instance != null)
            LivesManager.Instance.OnLivesChanged += OnLivesChanged;
    }

    private void OnDestroy()
    {
        if (LivesManager.Instance != null)
            LivesManager.Instance.OnLivesChanged -= OnLivesChanged;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Salud
    // ─────────────────────────────────────────────────────────────────────────

    public void UpdateHealthDisplay_Damage(int index)
    {
        int heartIndex = index / 2;
        if (heartIndex >= 0 && heartIndex < hearts.Count)
            hearts[heartIndex].UpdateHeartSprite_Damage(index);
    }

    public void UpdateHealthDisplay_Cure(int index)
    {
        int heartIndex = index / 2;
        if (heartIndex >= 0 && heartIndex < hearts.Count)
            hearts[heartIndex].UpdateHeartSprite_Cure(index);
    }

    public void ResetHearts()
    {
        foreach (var heart in hearts)
            heart.Init(heartEmpty, heartHalf, heartFull);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Gemas
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Actualiza el contador de gemas del nivel actual (formato X o X / Total).</summary>
    public void UpdateGems_UI(int currentGems, int totalGems = 0)
    {
        if (gemsLevelText != null)
        {
            gemsLevelText.text = totalGems > 0
                ? $"{currentGems} / {totalGems}"
                : currentGems.ToString();
        }
    }

    /// <summary>Refresca el texto de gemas globales del wallet.</summary>
    public void RefreshGlobalGemsUI()
    {
        if (gemsGlobalText == null || SaveManager.Instance == null) return;
        gemsGlobalText.text = SaveManager.Instance.LoadGlobalGems().ToString();
    }

    /// <summary>Refresca el contador de tréboles acumulados en el HUD.</summary>
    public void RefreshTrebolUI()
    {
        if (trebolCountText == null || SaveManager.Instance == null) return;
        trebolCountText.text = $"x{SaveManager.Instance.LoadTotalTrebols()}";
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Vidas
    // ─────────────────────────────────────────────────────────────────────────

    private void OnLivesChanged(int lives) => RefreshLivesUI();

    public void RefreshLivesUI()
    {
        if (livesText == null || SaveManager.Instance == null) return;
        livesText.text = $"x{SaveManager.Instance.LoadLives()}";
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Fin de nivel
    // ─────────────────────────────────────────────────────────────────────────

    public void Run_EndLevelAnimation()
    {
        if (endLevel_animator != null)
            endLevel_animator.gameObject.SetActive(true);
    }

    public void Run_AmazingJobAnimation()
    {
        if (amazingJob_animator == null) return;
        amazingJob_animator.gameObject.SetActive(true);
        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(TurnOffAmazingJob(2f));
    }

    /// <summary>
    /// Muestra las estrellas de calificación en la pantalla de fin de nivel.
    /// Llamado por LevelManager al completar el nivel.
    /// </summary>
    public void ShowLevelRating(LevelRating rating)
    {
        if (starImages == null || starImages.Count == 0) return;

        bool[] states = LevelRatingSystem.GetStarStates(rating, starImages.Count);
        for (int i = 0; i < starImages.Count; i++)
        {
            if (starImages[i] == null) continue;
            starImages[i].sprite = states[i] ? starFull : starEmpty;
            starImages[i].gameObject.SetActive(true);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers privados
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator TurnOffAmazingJob(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (amazingJob_animator != null)
            amazingJob_animator.gameObject.SetActive(false);
        currentCoroutine = null;
    }
}
