using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth _instance;

    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    [Header("Health Variables")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;

    [Header("Invincibility Variables")]
    [SerializeField] private float invicibilityCounter;
    [SerializeField] private float invincibilityLength;

    private Coroutine currentCoroutine = null;
    private bool isInvincible = false;

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
        currentHealth = maxHealth;

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        CheckpointManager._instance.UpdateCurrentPosition(transform.position);
    }

    public void DealDamage()
    {
        if (invicibilityCounter <= 0)
        {
            currentHealth--;
        
            //Validating
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                
                if (AnalyticsManager.Instance != null)
                    AnalyticsManager.Instance.TrackPlayerDeath(
                        SceneUtils.Get_CurrentLevelName(),
                        transform.position,
                        "damage" 
                    );
                
                LevelManager._instance.RespawnPlayer();
                AudioMixerManager._instance.ResetPitch();
            }
            else
            {
                invicibilityCounter = invincibilityLength;
          
                PlayerController2d._instance.KnockBack();
                AudioMixerManager._instance.CallSFX(SFXType.Player_Hurt);
                AudioMixerManager._instance.ResetPitch();
                
                isInvincible = true;
                if (currentCoroutine == null)
                    currentCoroutine = StartCoroutine(BlinkEffect());
            }
            UIController._instance.UpdateHealthDisplay_Damage(currentHealth);
        }
    }

    public void DeathEffect()
    {
        deathEffect.transform.position = this.transform.position;
        deathEffect.SetActive(true);
    }

    public bool isDeathEffectEnd()
    {
        return deathEffect.GetComponent<TurnOff_OnTime>().finish;
    }

    public bool isDamaged()
    {
        return currentHealth < maxHealth;
    }

    public void ResetDeathEffect()
    {
        deathEffect.GetComponent<TurnOff_OnTime>().Reset();
    }

    public void TurnOffPlayer()
    {                
        this.gameObject.SetActive(false);
        AudioMixerManager._instance.CallSFX(SFXType.Player_Death);
    }
    
    public void CureDamage()
    {
        if(currentHealth!=maxHealth)
            UIController._instance.UpdateHealthDisplay_Cure(currentHealth);
    
        currentHealth++;
        
        //Validating
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;            
        }
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
    }
    public void ResetPlayer()
    {
        transform.position = CheckpointManager._instance.GetPosition();
        Color tempColor = _spriteRenderer.color;
        _spriteRenderer.color = new Color(tempColor.r,
            tempColor.g,
            tempColor.b,
            1);
        gameObject.SetActive(true);
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
        CureDamage();   // Llenar la nueva salud máxima
        UIController._instance?.ResetHearts();
    }

    /// <summary>
    /// Quita 1 corazón bonus. Usado al iniciar una nueva partida o Game Over.
    /// </summary>
    public void RemoveBonusHeart()
    {
        if (bonusHearts <= 0) return;
        bonusHearts--;
        maxHealth = Mathf.Max(maxHealth - 2, 2); // mínimo 1 corazón base
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UIController._instance?.ResetHearts();
    }

    /// <summary>
    /// Resetea todos los corazones bonus (al iniciar nueva partida o Game Over definitivo).
    /// </summary>
    public void ResetBonusHearts()
    {
        if (bonusHearts <= 0) return;
        maxHealth -= bonusHearts * 2;
        maxHealth = Mathf.Max(maxHealth, 2);
        bonusHearts = 0;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UIController._instance?.ResetHearts();
    }

    /// <summary>Retorna cuántos corazones bonus están activos.</summary>
    public int GetBonusHearts() => bonusHearts;

    IEnumerator BlinkEffect()
    {
        Color tempColor = _spriteRenderer.color;
        stompBox.TurnLayer_Inactive();
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
        stompBox.TurnLayer_Active();
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
