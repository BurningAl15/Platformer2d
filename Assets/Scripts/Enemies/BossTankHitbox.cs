using UnityEngine;

public class BossTankHitbox : MonoBehaviour
{
    [SerializeField] private BossTankController bossCont;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("StompBox") && PlayerController2d._instance.transform.position.y > transform.position.y)
        {
            bossCont.TakeHit();
            PlayerController2d._instance.Bounce();
            gameObject.SetActive(false);
        }
    }
}
