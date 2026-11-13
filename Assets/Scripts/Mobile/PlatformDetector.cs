using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;

public class PlatformDetector : MonoBehaviour
{
    [Header("Mobile UI References")]
    [SerializeField] private GameObject gameplayMobileUI;
    [SerializeField] private GameObject levelSelectorMobileUI;
    
    [Header("Scene Detection Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string levelSelectorSceneName = "LevelSelection";
    [SerializeField] private string levelScenePrefix = "Level_";

    [Header("Debug")]
    [SerializeField] private bool forceMobileUI = false;
    private bool gameplayControlsSuppressed;
    
    public static PlatformDetector Instance { get; private set; }
    
    public static bool IsMobile()
    {
        if (Instance != null && Instance.forceMobileUI)
            return true;

        #if UNITY_ANDROID || UNITY_IOS
            return true;
        #else
            return Application.isMobilePlatform;
        #endif
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        EnsureCanvasSetup();
        
        // Suscribirse al evento de carga de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Configurar UI para la escena actual
        ConfigureMobileUIForCurrentScene();
    }

    private void Start()
    {
        // Reapply once scene Awake/OnEnable and Input System device discovery
        // have settled. This makes the persisted HUD state deterministic when
        // the app resumes directly into a gameplay scene.
        Invoke(nameof(ConfigureMobileUIForCurrentScene), 0.15f);
    }

    private void EnsureCanvasSetup()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
        }

        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            if (rt.localScale.x == 0f || rt.localScale.y == 0f || rt.localScale.z == 0f)
            {
                rt.localScale = Vector3.one;
            }
        }

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            int targetThreshold = Mathf.Max(30, (int)(Screen.dpi * 0.15f));
            if (UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold < targetThreshold)
            {
                UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold = targetThreshold;
            }
        }
    }
    
    private void OnDestroy()
    {
        // Desuscribirse del evento
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            StartCoroutine(ReapplyAfterSceneActivation(SceneManager.GetActiveScene().name));
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameplayControlsSuppressed = false;
        EnsureCanvasSetup();
        ConfigureMobileUIForScene(scene.name);
        StartCoroutine(ReapplyAfterSceneActivation(scene.name));
    }

    private IEnumerator ReapplyAfterSceneActivation(string sceneName)
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        ConfigureMobileUIForScene(sceneName);
    }

    private static bool ShouldShowTouchControls(bool isMobile)
    {
        // On mobile the virtual HUD is the default. A physical gamepad connection
        // event is the only thing that may hide it.
        return isMobile && (GamepadDetector.Instance == null || !GamepadDetector.Instance.HasPhysicalGamepad());
    }
    
    public void ConfigureMobileUIForCurrentScene()
    {
        ConfigureMobileUIForScene(SceneManager.GetActiveScene().name);
    }

    public void ConfigureMobileUIForScene(string currentSceneName)
    {
        bool isMobile = IsMobile();
        
        // Determinar qué UI activar según la escena
        if (IsMainMenuScene(currentSceneName))
        {
            // MainMenu: No activar ningún control mobile
            SetGameplayUI(false);
            SetLevelSelectorUI(false);
            
            Debug.Log("[PlatformDetector] MainMenu detected. Gameplay controls hidden.");
        }
        else if (IsLevelSelectorScene(currentSceneName))
        {
            // LevelSelector: En móviles se navega por gestos y toques directos en el mapa para no restar espacio en pantalla
            SetGameplayUI(false);
            SetLevelSelectorUI(false);
            
            Debug.Log("[PlatformDetector] Level selector detected. Gameplay controls hidden.");
        }
        else if (IsGameplayLevelScene(currentSceneName))
        {
            // Niveles de gameplay: Activar controles de gameplay en móvil (Stick, Jump, Pause)
            bool hasPhysicalGamepad = GamepadDetector.Instance != null &&
                                      GamepadDetector.Instance.HasPhysicalGamepad();
            bool showTouchControls = ShouldShowTouchControls(isMobile);
            SetGameplayUI(showTouchControls);
            SetLevelSelectorUI(false);

            LogGameplayControlsState(currentSceneName, isMobile, hasPhysicalGamepad, showTouchControls);
        }
        else
        {
            // Escena desconocida: Desactivar todo por seguridad
            SetGameplayUI(false);
            SetLevelSelectorUI(false);
            
            Debug.LogWarning($"[PlatformDetector] Unknown scene '{currentSceneName}'. Gameplay controls hidden.");
        }
    }
    
    private bool IsMainMenuScene(string sceneName)
    {
        return sceneName.Equals(mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase);
    }
    
    private bool IsLevelSelectorScene(string sceneName)
    {
        return sceneName.Contains("LevelSelection") || 
               sceneName.Equals(levelSelectorSceneName, System.StringComparison.OrdinalIgnoreCase);
    }
    
    private bool IsGameplayLevelScene(string sceneName)
    {
        // Detecta si la escena empieza con "Level_" (Level_0, Level_1, etc)
        return sceneName.StartsWith(levelScenePrefix, System.StringComparison.OrdinalIgnoreCase) ||
               sceneName.Contains("Level") && !sceneName.Contains("Selection");
    }
    
    private void SetGameplayUI(bool active)
    {
        active = active && !gameplayControlsSuppressed;
        if (gameplayMobileUI != null)
        {
            gameplayMobileUI.SetActive(active);
        }
    }

    private void LogGameplayControlsState(string sceneName, bool isMobile, bool hasPhysicalGamepad, bool requested)
    {
        if (gameplayMobileUI == null)
        {
            Debug.LogError($"[PlatformDetector] Gameplay controls are not assigned. scene='{sceneName}', mobile={isMobile}, physicalGamepad={hasPhysicalGamepad}.", this);
            return;
        }

        Canvas canvas = gameplayMobileUI.GetComponentInParent<Canvas>();
        var state = new StringBuilder();
        for (Transform current = gameplayMobileUI.transform; current != null; current = current.parent)
        {
            if (state.Length > 0)
                state.Append(" <- ");
            state.Append(current.name).Append("[").Append(current.gameObject.activeSelf ? "on" : "off").Append("]");
        }

        Debug.Log($"[PlatformDetector] Gameplay controls: scene='{sceneName}', mobile={isMobile}, physicalGamepad={hasPhysicalGamepad}, suppressed={gameplayControlsSuppressed}, requested={requested}, rootActive={gameplayMobileUI.activeSelf}, hierarchyActive={gameplayMobileUI.activeInHierarchy}, canvas={(canvas != null ? $"enabled={canvas.enabled}, renderMode={canvas.renderMode}, order={canvas.sortingOrder}" : "missing")}, hierarchy={state}.", gameplayMobileUI);
    }
    
    private void SetLevelSelectorUI(bool active)
    {
        if (levelSelectorMobileUI != null)
        {
            levelSelectorMobileUI.SetActive(active);
        }
    }
    
    // Método público para forzar reconfiguración (útil para debugging)
    public void ForceReconfigure()
    {
        ConfigureMobileUIForCurrentScene();
    }

    public void SetGameplayControlsSuppressed(bool suppressed)
    {
        gameplayControlsSuppressed = suppressed;
        ConfigureMobileUIForCurrentScene();
    }
    
    /// <summary>
    /// Activa/desactiva la UI mobile respetando la plataforma actual.
    /// Si IsMobile() es false, nunca activa ninguna UI mobile.
    /// </summary>
    public void SetMobileUIActive(bool gameplay, bool levelSelector)
    {
        SetGameplayUI(gameplay && ShouldShowTouchControls(IsMobile()));
        SetLevelSelectorUI(levelSelector && IsMobile());
    }

    /// <summary>
    /// Aplica un estado solicitado sin activar gameplay táctil fuera de mobile
    /// ni mientras haya un gamepad físico conectado.
    /// </summary>
    public void ForceSetMobileUIActive(bool gameplay, bool levelSelector)
    {
        SetGameplayUI(gameplay && ShouldShowTouchControls(IsMobile()));
        SetLevelSelectorUI(levelSelector);
    }
}
