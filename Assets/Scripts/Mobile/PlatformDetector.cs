using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureCanvasSetup();
        // Cada vez que carga una escena nueva, reconfigurar UI usando el nombre de la escena recién cargada
        ConfigureMobileUIForScene(scene.name);
    }

    private void Update()
    {
        if (IsMobile())
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (IsGameplayLevelScene(currentSceneName))
            {
                if (gameplayMobileUI != null && !gameplayMobileUI.activeSelf)
                {
                    SetGameplayUI(true);
                }
            }
        }
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
            
            Debug.Log($"[PlatformDetector] MainMenu detected. Mobile UI: OFF");
        }
        else if (IsLevelSelectorScene(currentSceneName))
        {
            // LevelSelector: En móviles se navega por gestos y toques directos en el mapa para no restar espacio en pantalla
            SetGameplayUI(false);
            SetLevelSelectorUI(false);
            
            Debug.Log($"[PlatformDetector] LevelSelector detected. Touch map navigation enabled, virtual sticks: OFF");
        }
        else if (IsGameplayLevelScene(currentSceneName))
        {
            // Niveles de gameplay: Activar controles de gameplay en móvil (Stick, Jump, Pause)
            SetGameplayUI(isMobile);
            SetLevelSelectorUI(false);
            
            Debug.Log($"[PlatformDetector] Gameplay Level '{currentSceneName}' detected. Gameplay UI: {isMobile}");
        }
        else
        {
            // Escena desconocida: Desactivar todo por seguridad
            SetGameplayUI(false);
            SetLevelSelectorUI(false);
            
            Debug.LogWarning($"[PlatformDetector] Unknown scene '{currentSceneName}'. Mobile UI: OFF");
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
        if (gameplayMobileUI != null)
        {
            gameplayMobileUI.SetActive(active);
            if (active)
            {
                EnsureChildrenActive(gameplayMobileUI);
            }
        }
    }
    
    private void SetLevelSelectorUI(bool active)
    {
        if (levelSelectorMobileUI != null)
        {
            levelSelectorMobileUI.SetActive(active);
            if (active)
            {
                EnsureChildrenActive(levelSelectorMobileUI);
            }
        }
    }

    private void EnsureChildrenActive(GameObject root)
    {
        if (root == null) return;
        Transform rootT = root.transform;
        for (int i = 0; i < rootT.childCount; i++)
        {
            Transform child = rootT.GetChild(i);
            child.gameObject.SetActive(true);
            for (int j = 0; j < child.childCount; j++)
            {
                child.GetChild(j).gameObject.SetActive(true);
            }
        }
    }
    
    // Método público para forzar reconfiguración (útil para debugging)
    public void ForceReconfigure()
    {
        ConfigureMobileUIForCurrentScene();
    }
    
    /// <summary>
    /// Activa/desactiva la UI mobile respetando la plataforma actual.
    /// Si IsMobile() es false, nunca activa ninguna UI mobile.
    /// </summary>
    public void SetMobileUIActive(bool gameplay, bool levelSelector)
    {
        SetGameplayUI(gameplay && IsMobile());
        SetLevelSelectorUI(levelSelector && IsMobile());
    }

    /// <summary>
    /// Fuerza el estado de la UI mobile independientemente de la plataforma.
    /// Usar cuando GamepadDetector necesita ocultar controles en mobile.
    /// </summary>
    public void ForceSetMobileUIActive(bool gameplay, bool levelSelector)
    {
        SetGameplayUI(gameplay);
        SetLevelSelectorUI(levelSelector);
    }
}
