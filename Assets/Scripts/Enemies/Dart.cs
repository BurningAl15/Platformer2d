using UnityEngine;

public class Dart : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private int direction;

    [SerializeField] private GameObject smoke;

    [SerializeField] private Rigidbody2D rgb;
    void OnEnable()
    {
        rgb = GetComponent<Rigidbody2D>();
        // OnEnable can run before the audio manager on scene load.
        AudioMixerManager._instance?.CallSFX(SFXType.Boss_Shoot);
        rgb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        rgb.linearVelocity += speed * direction * Time.deltaTime * Vector2.right;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth._instance.DealDamage();
            PlayerController2d._instance.KnockBack();
            SpawnFactory.Release(gameObject);
            ScreenShake._instance.StartShake(.3f,.5f);
        }
        if(other.CompareTag("Ground") || other.CompareTag("Platform"))
            SpawnFactory.Release(gameObject);
        SpawnFactory.Spawn(smoke, transform.position, transform.rotation);
        AudioMixerManager._instance.CallSFX(SFXType.Boss_Impact);
    }
}
