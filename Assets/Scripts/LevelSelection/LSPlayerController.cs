using UnityEngine;

/// <summary>
/// Controlador del jugador en la pantalla de selección de niveles.
/// Navega entre MapPoints con el InputManager y proporciona feedback
/// visual (Select/Deselect en MapPoint) y sonoro.
/// </summary>
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

    public void SetCurrentPoint(MapPoint newPoint)
    {
        // Deseleccionar el punto anterior si había uno
        if (currentPoint != null)
            currentPoint.Deselect();

        currentPoint = newPoint;
        transform.position = currentPoint.transform.position;
        currentPoint.Select();
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentPoint.transform.position,
            moveSpeed * Time.deltaTime);

        float currentDistance = Vector3.Distance(transform.position, currentPoint.transform.position);

        // Actualizar cooldown
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
        else
            hasProcessedInput = false;

        // Procesar navegación solo al estar cerca del nodo
        if (currentDistance < 0.025f)
        {
            HandleNavigation();

            // Mostrar panel de info solo cuando estamos exactamente sobre el nodo
            if (currentDistance <= 0.01f)
            {
                if (currentPoint.isLevel && !currentPoint.isLocked)
                    UI_SelectController._instance.Turn_On_Off(true);
            }
        }
    }

    private void HandleNavigation()
    {
        if (InputManager.Instance == null) return;

        Vector2 moveInput = InputManager.Instance.MoveInput;

        if (moveInput.magnitude < 0.1f)
        {
            previousMoveInput = Vector2.zero;
            return;
        }

        if (hasProcessedInput || cooldownTimer > 0) return;

        MapPoint nextPoint = null;

        if (moveInput.x > 0.5f)
            nextPoint = currentPoint.right;
        else if (moveInput.x < -0.5f)
            nextPoint = currentPoint.left;
        else if (moveInput.y > 0.5f)
            nextPoint = currentPoint.up;
        else if (moveInput.y < -0.5f)
            nextPoint = currentPoint.down;

        if (nextPoint != null)
        {
            SetNextPoint(nextPoint);
            hasProcessedInput = true;
            cooldownTimer = inputCooldown;
            previousMoveInput = moveInput;
        }
    }

    private void HandleLevelSelect()
    {
        if (currentPoint == null) return;

        float dist = Vector3.Distance(transform.position, currentPoint.transform.position);
        if (dist > 0.01f) return;

        if (currentPoint.isLevel && !currentPoint.isLocked)
        {
            AudioMixerManager._instance.CallSFX(SFXType.Level_Selected);
            LevelSelectManager._instance.Loading_GameplayScene(currentPoint.currentLevel);
        }
        else if (currentPoint.isLocked)
        {
            // Feedback sonoro de nivel bloqueado (usa Warp_Jingle como sonido de "no disponible")
            // Cuando tengamos un SFX dedicado, reemplazar aquí.
            AudioMixerManager._instance.CallSFX(SFXType.Warp_Jingle);
            Debug.Log($"[LSPlayerController] Nivel bloqueado: {currentPoint.levelName}");
        }
    }

    private void SetNextPoint(MapPoint nextPoint)
    {
        // Deseleccionar el punto actual
        currentPoint.Deselect();

        // Ocultar panel de info
        UI_SelectController._instance.Turn_On_Off(false);

        // Reproducir sonido de navegación
        AudioMixerManager._instance.CallSFX(SFXType.Map_Movement);

        currentPoint = nextPoint;
        currentPoint.Select();

        Debug.Log($"[LSPlayerController] Navegando a: {nextPoint.name}");
    }
}
