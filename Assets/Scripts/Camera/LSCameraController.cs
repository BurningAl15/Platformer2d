using UnityEngine;

public class LSCameraController : MonoBehaviour
{
    public Vector2 minPos, maxPos;
    public Transform target;
    
    [Header("Auto Find Settings")]
    [SerializeField] private bool autoFindTarget = true;
    [SerializeField] private string targetTag = "Player";

    void LateUpdate()
    {
        if (target == null && autoFindTarget)
        {
            FindTarget();
        }

        if (target == null)
        {
            return;
        }

        float xPos = Mathf.Clamp(target.position.x, minPos.x, maxPos.x);
        float yPos = Mathf.Clamp(target.position.y, minPos.y, maxPos.y);

        transform.position = new Vector3(xPos, yPos, transform.position.z);
    }

    void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag(targetTag);
        if (player != null)
        {
            target = player.transform;
            Debug.Log($"LSCameraController: Found target '{player.name}'");
        }
    }
}