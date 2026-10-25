using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTankController : MonoBehaviour
{
    public enum BossStates
    {
        SHOOTING,
        HURT,
        MOVING,

    };

    public BossStates currentStates;
    
    [Header("Components")]
    [SerializeField] private Transform boss;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    [Header("Movement Variables")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool moveRight;

    [Header("Points")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    [Header("Shooting Variables")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float timeBetweenShots;
    private float shootCounter;
    
    [Header("Hurt Variables")]
    [SerializeField] private float hurtTime;
    private float hurtCounter;
    public GameObject hitbox;
    
    [Header("Mine Variables")]
    [SerializeField] private GameObject mine;
    [SerializeField] private Transform minePoint;
    [SerializeField] private float timeBetweenMines;
    private float mineCounter;

    [Header("Health")] 
    public int health = 5;

    public GameObject explosion;
    public GameObject winPlatform;
    private bool isDefeated;
    public float shotSpeedUp, mineSpeedUp;

    private Coroutine currentCoroutine = null;
    private bool runOnce;
    
    void Start()
    {
        currentStates = BossStates.SHOOTING;
        // leftPoint.parent = rightPoint.parent = null;
        hitbox.SetActive(false);
        winPlatform.SetActive(false);
    }

    void Update()
    {
        if (!isDefeated)
        {
            switch (currentStates)
            {
                case BossStates.SHOOTING:
                    Shooting_State();
                    
                    break;
                case BossStates.HURT:
                    Hurt_State();
                    
                    break;
                case BossStates.MOVING:
                    Moving_State();
                    
                    break;
            }
        }
        else
        {
            if (!runOnce)
            {
                End_State();
            }
        }
    }

    void Shooting_State()
    {
        hitbox.SetActive(true);
        shootCounter -= Time.deltaTime;
        if (shootCounter <= 0)
        {
            shootCounter = timeBetweenShots;
            var newBullet = Instantiate(bullet, firePoint.position, firePoint.rotation);
            newBullet.transform.eulerAngles = new Vector3(0, moveRight ? 180 : 0, 0);
        }
    }

    void Hurt_State()
    {
        if (hurtCounter > 0)
        {
            hurtCounter -= Time.deltaTime;
            if (hurtCounter <= 0)
            {
                currentStates = BossStates.MOVING;
                mineCounter = 0;
            }
        }
    }
    
    void Moving_State()
    {
        hitbox.SetActive(false);
        if (moveRight)
        {
            boss.localPosition += new Vector3(moveSpeed * Time.deltaTime, 0f, 0f);
            if (boss.localPosition.x > rightPoint.localPosition.x)
                EndMovement(false);
        }
        else
        {
            boss.localPosition -= new Vector3(moveSpeed * Time.deltaTime, 0f, 0f);
            if (boss.localPosition.x < leftPoint.localPosition.x)
                EndMovement(true);
        }

        MineTimer();
    }

    void End_State()
    {
        //Activate win platform
        winPlatform.transform.parent = null;
        winPlatform.SetActive(true);
        //Stop boss music (fade to main music again)
        AudioMixerManager._instance.StopBossBackground();
        gameObject.SetActive(false);
        Instantiate(explosion, transform.position, transform.rotation);
        UIController._instance.Run_AmazingJobAnimation();
        runOnce = true;
    }
    
    void MineTimer()
    {
        mineCounter -= Time.deltaTime;
        if (mineCounter <= 0)
        {
            mineCounter = timeBetweenMines;
            Instantiate(mine, minePoint.position, minePoint.rotation);
        }
    }
    
    public void TakeHit()
    {
        currentStates = BossStates.HURT;
        hurtCounter = hurtTime;
        
        AudioMixerManager._instance.CallSFX(SFXType.Boss_Hit);

        anim.SetTrigger("Hit");

        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(BlinkEffect(.25f));
        
        BossTankMine[] mines = FindObjectsOfType<BossTankMine>();
        if (mines.Length > 0)
            foreach (BossTankMine foundMine in mines)
                foundMine.DestroyMine();

        BossHealth();
    }

    void BossHealth()
    {
        health--;
        if (health <= 0)
        {
            AudioMixerManager._instance.CallSFX(SFXType.Enemy_Death);
            isDefeated = true;
        }
        else
        {
            timeBetweenShots /= shotSpeedUp;
            timeBetweenMines /= mineSpeedUp;
        }
    }

    void FLip()
    {
        boss.eulerAngles = new Vector3(0, moveRight ? 180 : 0, 0);
    }

    void EndMovement(bool _moveRight)
    {
        moveRight = _moveRight;
        currentStates = BossStates.SHOOTING;
        shootCounter = 0f;
        anim.SetTrigger("StopMoving");
        FLip();
        hitbox.SetActive(true);
    }

 
    
    IEnumerator BlinkEffect(float blinkTime)
    {
        Color tempColor = _spriteRenderer.color;
        float blinkCounter = blinkTime;
        // while (isInvincible)
        while (blinkCounter>=0)
        {
            blinkCounter -= Time.deltaTime;
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

        _spriteRenderer.color = tempColor;
        currentCoroutine = null;
    }
}
