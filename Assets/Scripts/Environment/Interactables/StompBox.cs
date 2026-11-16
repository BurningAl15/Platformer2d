using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class StompBox : MonoBehaviour
{
    [SerializeField] private GameObject collectible;
    [Range(0, 100)] [SerializeField] private float chanceToDrop;
    [SerializeField, Min(0f)] private float minimumDownwardSpeed = 0.05f;
    [SerializeField, Min(0f)] private float maximumTopContactDepth = 0.35f;

    private Bouncer tempBouncer = null;
    private Collider2D stompCollider;
    private Rigidbody2D playerBody;
    private bool wasDescendingAtLastPhysicsStep;

    private void Awake()
    {
        stompCollider = GetComponent<Collider2D>();
        playerBody = GetComponentInParent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        wasDescendingAtLastPhysicsStep = playerBody != null &&
                                         playerBody.linearVelocity.y < -minimumDownwardSpeed;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(StringUtils.tag_Enemy) && IsValidTopStomp(other))
            EnemyBehaviour(other);

        if (other.CompareTag(StringUtils.tag_Bouncer))
        {
            float bounceValue = 2f;
            if (other.GetComponent<Bouncer>() != null)
            {
                tempBouncer = other.GetComponent<Bouncer>();
                bounceValue = tempBouncer.bouncePower;
            }

            PlayerController2d._instance.Bounce(bounceValue);
            other.GetComponent<Animator>().SetTrigger("Bounce");
        }
    }

    private bool IsValidTopStomp(Collider2D enemyCollider)
    {
        if (!wasDescendingAtLastPhysicsStep || stompCollider == null || enemyCollider.isTrigger)
            return false;

        Bounds stompBounds = stompCollider.bounds;
        Bounds enemyBounds = enemyCollider.bounds;
        bool overlapsHorizontally = stompBounds.max.x > enemyBounds.min.x &&
                                    stompBounds.min.x < enemyBounds.max.x;
        float penetrationPastEnemyTop = enemyBounds.max.y - stompBounds.min.y;

        return overlapsHorizontally &&
               penetrationPastEnemyTop >= -0.02f &&
               penetrationPastEnemyTop <= maximumTopContactDepth;
    }

    private void EnemyBehaviour(Collider2D other)
    {
        EnemyParent enemy = other.GetComponentInParent<EnemyParent>();
        if (enemy == null || enemy.IsDying)
            return;

        Vector2 lastPos = other.transform.position;
        if (other.transform != enemy.transform)
            other.gameObject.SetActive(false);
        enemy.Death();
        
        PlayerController2d._instance.Bounce();
        AppBootstrap.TriggerHapticFeedback();
        AudioMixerManager._instance.CallSFX(SFXType.Enemy_Death);

        DropItem(lastPos);
    }

    private void DropItem(Vector2 _lastPos)
    {
        float dropSelect = Random.Range(0, 100);

        if (dropSelect <= chanceToDrop)
            Instantiate(collectible, _lastPos, Quaternion.identity);
    }

    public void TurnLayer_Active()
    {
        gameObject.layer = 10;
    }

    public void TurnLayer_Inactive()
    {
        gameObject.layer = 13;
    }
}
