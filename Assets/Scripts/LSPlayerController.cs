using UnityEngine;

public class LSPlayerController : MonoBehaviour
{
    public MapPoint currentPoint;
    public float moveSpeed = 10f;
    
    [Header("Input Settings")]
    [Tooltip("Tiempo mínimo entre movimientos (en segundos)")]
    public float inputCooldown = 0.3f;
    
    private bool hasProcessedInput = false;
    private float cooldownTimer = 0f;
    private Vector2 previousMoveInput;
    
    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed += HandleLevelSelect;
            InputManager.Instance.OnSubmitPressed += HandleLevelSelect;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed -= HandleLevelSelect;
            InputManager.Instance.OnSubmitPressed -= HandleLevelSelect;
        }
    }

    public void SetCurrentPoint(MapPoint _currentMapPoint)
    {
        currentPoint = _currentMapPoint;
        transform.position = currentPoint.transform.position;
    }
    
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentPoint.transform.position,
            moveSpeed * Time.deltaTime);

        float currentDistance = Vector3.Distance(transform.position, currentPoint.transform.position);
        
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else
        {
            hasProcessedInput = false;
        }
        
        if (currentDistance < .025f)
        {
            HandleNavigation();
            
            if (currentDistance <= 0.01f)
            {
                if (currentPoint.isLevel && !currentPoint.isLocked)
                {
                    UI_SelectController._instance.Turn_On_Off(true);
                    UI_SelectController._instance.RenderLevelData(currentPoint);
                }
            }
        }
    }

    void HandleNavigation()
    {
        if (InputManager.Instance == null)
            return;

        Vector2 moveInput = InputManager.Instance.MoveInput;
        
        if (moveInput.magnitude < 0.1f)
        {
            previousMoveInput = Vector2.zero;
            return;
        }
        
        if (hasProcessedInput || cooldownTimer > 0)
            return;

        bool inputChanged = Vector2.Dot(moveInput.normalized, previousMoveInput.normalized) < 0.9f;
        
        if (moveInput.x > 0.5f)
        {
            if (currentPoint.right != null)
            {
                SetNextPoint(currentPoint.right);
                hasProcessedInput = true;
                cooldownTimer = inputCooldown;
                previousMoveInput = moveInput;
            }
        }
        else if (moveInput.x < -0.5f)
        {
            if (currentPoint.left != null)
            {
                SetNextPoint(currentPoint.left);
                hasProcessedInput = true;
                cooldownTimer = inputCooldown;
                previousMoveInput = moveInput;
            }
        }
        else if (moveInput.y > 0.5f)
        {
            if (currentPoint.up != null)
            {
                SetNextPoint(currentPoint.up);
                hasProcessedInput = true;
                cooldownTimer = inputCooldown;
                previousMoveInput = moveInput;
            }
        }
        else if (moveInput.y < -0.5f)
        {
            if (currentPoint.down != null)
            {
                SetNextPoint(currentPoint.down);
                hasProcessedInput = true;
                cooldownTimer = inputCooldown;
                previousMoveInput = moveInput;
            }
        }
    }

    void HandleLevelSelect()
    {
        if (currentPoint == null)
            return;
            
        float currentDistance = Vector3.Distance(transform.position, currentPoint.transform.position);
        
        if (currentDistance <= 0.01f)
        {
            if (currentPoint.isLevel && !currentPoint.isLocked)
            {
                Debug.Log($"Selecting level: {currentPoint.currentLevel}");
                LevelSelectManager._instance.Loading_GameplayScene(currentPoint.currentLevel);
            }
            else
            {
                Debug.Log($"Cannot select: isLevel={currentPoint.isLevel}, isLocked={currentPoint.isLocked}");
            }
        }
    }

    void SetNextPoint(MapPoint nextPoint)
    {
        currentPoint = nextPoint;
        UI_SelectController._instance.Turn_On_Off(false);
        AudioMixerManager._instance.CallSFX(SFXType.Map_Movement);
        Debug.Log($"Moving to point: {nextPoint.name}");
    }
}
