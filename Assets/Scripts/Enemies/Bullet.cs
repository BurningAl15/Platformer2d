using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private int direction;

    [SerializeField] private GameObject smoke;
    void OnEnable()
    {
        // OnEnable can run before the audio manager on scene load.
        AudioMixerManager._instance?.CallSFX(SFXType.Boss_Shoot);
    }

    void Update()
    {
        direction = Mathf.Approximately(transform.eulerAngles.y, 180) ? -1 : 1;
        transform.position += new Vector3(-speed * direction * Time.deltaTime, 0f);
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
        SpawnFactory.Spawn(smoke, transform.position, transform.rotation);
        AudioMixerManager._instance.CallSFX(SFXType.Boss_Impact);
    }
}
