using System.Collections.Generic;
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
    private bool currentPointInfoRefreshed;
    private float cooldownTimer = 0f;
    private Vector2 previousMoveInput;

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed += HandleLevelSelect;
            InputManager.Instance.OnPausePressed += OpenSettingsFromGamepad;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed -= HandleLevelSelect;
            InputManager.Instance.OnPausePressed -= OpenSettingsFromGamepad;
        }
    }

    private void OpenSettingsFromGamepad()
    {
        SettingsController settings = Object.FindFirstObjectByType<SettingsController>();
        if (settings != null)
            settings.OpenSettings();
    }

    public void SetCurrentPoint(MapPoint newPoint)
    {
        // Deseleccionar el punto anterior si había uno
        if (currentPoint != null)
            currentPoint.Deselect();

        currentPoint = newPoint;
        transform.position = currentPoint.transform.position;
        currentPoint.Select();
        RefreshSelectedLevelInfo();
    }

    private readonly Queue<MapPoint> pathQueue = new Queue<MapPoint>();
    public bool IsMovingAlongPath => pathQueue.Count > 0;

    private void Update()
    {
        if (currentPoint == null) return;

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

        // Procesar avance en nodo
        if (currentDistance < 0.025f)
        {
            if (pathQueue.Count > 0)
            {
                // Avanzar al siguiente nodo de la ruta trazada por toque en el mapa
                MapPoint next = pathQueue.Dequeue();
                SetNextPoint(next);
            }
            else
            {
                HandleNavigation();

                // Mostrar panel de info solo cuando estamos exactamente sobre el nodo
                if (currentDistance <= 0.01f)
                {
                    if (!currentPointInfoRefreshed)
                        RefreshSelectedLevelInfo();
                }
            }
        }

        UpdateTouchMapInteraction(currentDistance);
    }

    private void HandleNavigation()
    {
        if (pathQueue.Count > 0) return;
        if (InputManager.Instance == null) return;

        Vector2 moveInput = InputManager.Instance.MoveInput;

        if (moveInput.magnitude < 0.1f)
        {
            previousMoveInput = Vector2.zero;
            return;
        }

        TryNavigate(moveInput);
    }

    public bool TryNavigate(Vector2 moveInput)
    {
        if (hasProcessedInput || cooldownTimer > 0 || currentPoint == null || pathQueue.Count > 0) return false;

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
            return true;
        }

        return false;
    }

    /// <summary>
    /// Traza la ruta más corta (BFS) y hace que Fox avance automáticamente hasta el punto tocado.
    /// Si el punto tocado es el punto actual, confirma la selección de nivel.
    /// </summary>
    public bool NavigateToPoint(MapPoint target)
    {
        if (target == null || currentPoint == null) return false;

        if (target == currentPoint)
        {
            // Tocado el mismo nodo: si es nivel, entrar directamente
            HandleLevelSelect();
            return true;
        }

        if (target.isLocked)
        {
            if (AudioMixerManager._instance != null)
                AudioMixerManager._instance.CallSFX(SFXType.Warp_Jingle);
            Debug.Log($"[LSPlayerController] Nivel bloqueado: {target.levelName}");
            return false;
        }

        // Búsqueda en anchura (BFS) sobre el grafo de MapPoints conectados
        Queue<MapPoint> queue = new Queue<MapPoint>();
        Dictionary<MapPoint, MapPoint> cameFrom = new Dictionary<MapPoint, MapPoint>();
        HashSet<MapPoint> visited = new HashSet<MapPoint>();

        queue.Enqueue(currentPoint);
        visited.Add(currentPoint);

        bool found = false;
        while (queue.Count > 0)
        {
            MapPoint curr = queue.Dequeue();
            if (curr == target)
            {
                found = true;
                break;
            }

            MapPoint[] neighbors = new MapPoint[] { curr.right, curr.left, curr.up, curr.down };
            foreach (var n in neighbors)
            {
                if (n != null && !visited.Contains(n) && !n.isLocked)
                {
                    visited.Add(n);
                    cameFrom[n] = curr;
                    queue.Enqueue(n);
                }
            }
        }

        if (!found)
        {
            Debug.LogWarning($"[LSPlayerController] No hay ruta conectada hacia: {target.name}");
            return false;
        }

        // Reconstruir camino
        List<MapPoint> path = new List<MapPoint>();
        MapPoint step = target;
        while (step != currentPoint)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Reverse();

        pathQueue.Clear();
        foreach (var p in path)
        {
            pathQueue.Enqueue(p);
        }

        if (pathQueue.Count > 0)
        {
            MapPoint first = pathQueue.Dequeue();
            SetNextPoint(first);
        }

        return true;
    }

    private Vector2 touchStartPos;
    private bool isTouching;
    private float touchStartTime;

    private void UpdateTouchMapInteraction(float currentDistance)
    {
        if (pathQueue.Count > 0) return;

        // 1. Detección táctil en móviles
        var ts = UnityEngine.InputSystem.Touchscreen.current;
        if (ts != null)
        {
            var touch = ts.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                touchStartPos = touch.position.ReadValue();
                isTouching = true;
                touchStartTime = Time.unscaledTime;
            }
            else if (touch.press.wasReleasedThisFrame && isTouching)
            {
                isTouching = false;
                Vector2 releasePos = touch.position.ReadValue();
                Vector2 delta = releasePos - touchStartPos;
                float duration = Time.unscaledTime - touchStartTime;

                // Toque directo (TAP / CLICK)
                if (delta.magnitude < 35f && duration < 0.45f)
                {
                    ProcessScreenTap(releasePos);
                    return;
                }

                // Deslizamiento con el dedo (SWIPE)
                if (delta.magnitude >= 40f && currentDistance < 0.025f)
                {
                    Vector2 dir = Vector2.zero;
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        dir = delta.x > 0 ? Vector2.right : Vector2.left;
                    else
                        dir = delta.y > 0 ? Vector2.up : Vector2.down;

                    TryNavigate(dir);
                }
            }
        }

        // 2. Ratón en Unity Editor / PC
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null && mouse.leftButton.wasReleasedThisFrame)
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            Vector2 mousePos = mouse.position.ReadValue();
            ProcessScreenTap(mousePos);
        }
    }

    private void ProcessScreenTap(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        MapPoint[] allPoints = Object.FindObjectsByType<MapPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        MapPoint closest = null;
        float minDistance = 1.3f;

        foreach (var p in allPoints)
        {
            if (p == null) continue;
            float d = Vector2.Distance(worldPos, p.transform.position);
            if (d < minDistance)
            {
                minDistance = d;
                closest = p;
            }
        }

        if (closest != null)
        {
            NavigateToPoint(closest);
        }
    }

    public void HandleLevelSelect()
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
        if (currentPoint == null || nextPoint == null) return;

        // Deseleccionar el punto actual
        currentPoint.Deselect();

        // Ocultar panel de info
        currentPointInfoRefreshed = false;
        if (UI_SelectController._instance != null)
            UI_SelectController._instance.Turn_On_Off(false);

        // Reproducir sonido de navegación
        AudioMixerManager._instance?.CallSFX(SFXType.Map_Movement);

        currentPoint = nextPoint;
        currentPoint.Select();

        Debug.Log($"[LSPlayerController] Navegando a: {nextPoint.name}");
    }

    private void RefreshSelectedLevelInfo()
    {
        if (UI_SelectController._instance == null || currentPoint == null) return;

        if (currentPoint.isLevel && !currentPoint.isLocked)
        {
            UI_SelectController._instance.RenderLevelData(currentPoint);
            UI_SelectController._instance.Turn_On_Off(true);
        }
        else
        {
            UI_SelectController._instance.Turn_On_Off(false);
        }

        currentPointInfoRefreshed = true;
    }
}
