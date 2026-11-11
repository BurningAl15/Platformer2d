using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth _instance;

    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    [Header("Health Variables")]
    [SerializeField] private int currentHealth;
    [SerializeField, Min(1)] private int baseMaxHealth = 6;
    private int maxHealth;

    [Header("Invincibility Variables")]
    [SerializeField] private float invicibilityCounter;
    [SerializeField] private float invincibilityLength;

    private Coroutine currentCoroutine = null;
    private bool isInvincible = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    [Header("Death Effect")] 
    [SerializeField] private GameObject deathEffect;

    [SerializeField] private StompBox stompBox;

    // ─────────────────────────────────────────────────────────────────────────
    //  Corazones bonus (Dado Secreto 🎲)
    // ─────────────────────────────────────────────────────────────────────────
    private int bonusHearts = 0;
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        maxHealth = Mathf.Max(2, baseMaxHealth);
        currentHealth = maxHealth;

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Load permanent heart progression once scene services have completed Awake.
        int permanentHearts = SaveManager.Instance != null
            ? SaveManager.Instance.LoadPermanentHeartCount()
            : 0;
        maxHealth = Mathf.Max(2, baseMaxHealth) + permanentHearts * 2 + bonusHearts * 2;
        currentHealth = maxHealth;
        CheckpointManager._instance.UpdateCurrentPosition(transform.position);
        UIController._instance?.RefreshHealthDisplay();
    }

    public void DealDamage()
    {
        ApplyDamage(false, "damage");
    }

    /// <summary>Aplica daño de caída y devuelve al jugador al último waypoint.</summary>
    public void DealFallDamage()
    {
        // A fall still needs to escape the death volume during the invulnerability
        // window; it simply does not remove another health unit yet.
        if (invicibilityCounter > 0f)
        {
            LevelManager._instance?.RespawnPlayerFromFall();
            return;
        }
        ApplyDamage(true, "fall");
    }

    private void ApplyDamage(bool returnToCheckpoint, string cause)
    {
        if (invicibilityCounter > 0f || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - 1);
        UIController._instance?.RefreshHealthDisplay();

        if (currentHealth == 0)
        {
            if (AnalyticsManager.Instance != null)
                AnalyticsManager.Instance.TrackPlayerDeath(
                    SceneUtils.Get_CurrentLevelName(), transform.position, cause);

            LevelManager._instance?.RespawnPlayer();
            if (AudioMixerManager._instance != null)
                AudioMixerManager._instance.ResetPitch();
            return;
        }

        // The same short invulnerability window protects the player from repeated
        // trigger callbacks after a hit or fall.
        invicibilityCounter = Mathf.Max(0f, invincibilityLength);
        isInvincible = true;
        if (currentCoroutine == null && _spriteRenderer != null)
            currentCoroutine = StartCoroutine(BlinkEffect());

        if (AudioMixerManager._instance != null)
        {
            AudioMixerManager._instance.CallSFX(SFXType.Player_Hurt);
            AudioMixerManager._instance.ResetPitch();
        }

        if (returnToCheckpoint)
            LevelManager._instance?.RespawnPlayerFromFall();
        else
            PlayerController2d._instance?.KnockBack();
    }

    public void DeathEffect()
    {
        if (deathEffect == null) return;
        deathEffect.transform.position = this.transform.position;
        deathEffect.SetActive(true);
    }

    public bool isDeathEffectEnd()
    {
        if (deathEffect == null) return true;
        TurnOff_OnTime effectTimer = deathEffect.GetComponent<TurnOff_OnTime>();
        return effectTimer == null || effectTimer.finish;
    }

    public bool isDamaged()
    {
        return currentHealth < maxHealth;
    }

    public void ResetDeathEffect()
    {
        if (deathEffect == null) return;
        TurnOff_OnTime effectTimer = deathEffect.GetComponent<TurnOff_OnTime>();
        if (effectTimer != null)
            effectTimer.Reset();
    }

    public void TurnOffPlayer()
    {                
        this.gameObject.SetActive(false);
        if (AudioMixerManager._instance != null)
            AudioMixerManager._instance.CallSFX(SFXType.Player_Death);
    }
    
    public void CureDamage()
    {
        currentHealth++;
        
        //Validating
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;            
        }

        UIController._instance?.RefreshHealthDisplay();
    }
    
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        //     CureDamage();

        if (invicibilityCounter > 0)
        {
            invicibilityCounter -= Time.deltaTime;
            
            if (invicibilityCounter <= 0)
                isInvincible = false;
        }
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UIController._instance?.RefreshHealthDisplay();
    }
    public void ResetPlayer()
    {
        transform.position = CheckpointManager._instance.GetPosition();
        currentCoroutine = null; // deactivation stops the prior blink coroutine
        Color tempColor = _spriteRenderer.color;
        _spriteRenderer.color = new Color(tempColor.r,
            tempColor.g,
            tempColor.b,
            1);
        gameObject.SetActive(true);
        invicibilityCounter = Mathf.Max(invicibilityCounter, invincibilityLength);
        isInvincible = invicibilityCounter > 0f;
        if (stompBox != null) stompBox.TurnLayer_Active();
        if (isInvincible && currentCoroutine == null && _spriteRenderer != null)
            currentCoroutine = StartCoroutine(BlinkEffect());
        if (AudioMixerManager._instance != null)
            AudioMixerManager._instance.CallSFX(SFXType.Warp_Jingle);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Corazones bonus — otorgados por el Dado Secreto
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Agrega 1 corazón bonus temporal (aumenta maxHealth por esta sesión).
    /// Efecto del Dado Secreto según el GDD.
    /// </summary>
    public void AddBonusHeart()
    {
        bonusHearts++;
        maxHealth += 2; // 1 corazón = 2 unidades de salud (mitades)
        currentHealth = Mathf.Min(maxHealth, currentHealth + 2);
        UIController._instance?.RefreshHealthDisplay();
    }

    /// <summary>Aplica un corazón permanente ya registrado por SaveManager.</summary>
    public void AddPermanentHeart()
    {
        maxHealth += 2;
        currentHealth = Mathf.Min(maxHealth, currentHealth + 2);
        UIController._instance?.RefreshHealthDisplay();
    }

    /// <summary>
    /// Quita 1 corazón bonus. Usado al iniciar una nueva partida o Game Over.
    /// </summary>
    public void RemoveBonusHeart()
    {
        if (bonusHearts <= 0) return;
        bonusHearts--;
        maxHealth = Mathf.Max(maxHealth - 2, Mathf.Max(2, baseMaxHealth) +
            (SaveManager.Instance != null ? SaveManager.Instance.LoadPermanentHeartCount() * 2 : 0));
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UIController._instance?.RefreshHealthDisplay();
    }

    /// <summary>
    /// Resetea todos los corazones bonus (al iniciar nueva partida o Game Over definitivo).
    /// </summary>
    public void ResetBonusHearts()
    {
        if (bonusHearts <= 0) return;
        maxHealth -= bonusHearts * 2;
        maxHealth = Mathf.Max(maxHealth, Mathf.Max(2, baseMaxHealth) +
            (SaveManager.Instance != null ? SaveManager.Instance.LoadPermanentHeartCount() * 2 : 0));
        bonusHearts = 0;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UIController._instance?.RefreshHealthDisplay();
    }

    /// <summary>Retorna cuántos corazones bonus están activos.</summary>
    public int GetBonusHearts() => bonusHearts;

    IEnumerator BlinkEffect()
    {
        if (_spriteRenderer == null)
        {
            currentCoroutine = null;
            yield break;
        }

        Color tempColor = _spriteRenderer.color;
        if (stompBox != null) stompBox.TurnLayer_Inactive();
        while (isInvincible)
        {
            _spriteRenderer.color = new Color(tempColor.r,
                tempColor.g,
                tempColor.b,
                .5f);
            yield return new WaitForSeconds(.1f);
            _spriteRenderer.color = new Color(tempColor.r,
                tempColor.g,
                tempColor.b,
                1);
            yield return new WaitForSeconds(.1f);
        }
        if (stompBox != null) stompBox.TurnLayer_Active();
        currentCoroutine = null;
    }

    // private void OnCollisionEnter2D(Collision2D other)
    // {
    //     if (other.gameObject.CompareTag("Platform") && transform.parent == null && IsAlive())
    //         transform.parent = other.transform;
    // }
    //
    // private void OnCollisionExit2D(Collision2D other)
    // {
    //     if (other.gameObject.CompareTag("Platform") && transform.parent != null && IsAlive())
    //         transform.parent = null;
    // }
}
