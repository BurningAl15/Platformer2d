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
    
    public static PlatformDetector Instance { get; private set; }
    
    public static bool IsMobile()
    {
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
        
        // Suscribirse al evento de carga de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Configurar UI para la escena actual
        ConfigureMobileUIForCurrentScene();
    }
    
    private void OnDestroy()
    {
        // Desuscribirse del evento
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cada vez que carga una escena nueva, reconfigurar UI
        ConfigureMobileUIForCurrentScene();
    }
    
    private void ConfigureMobileUIForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
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
            // LevelSelector: Activar UI específico de selector (si existe y es mobile)
            SetGameplayUI(false);
            SetLevelSelectorUI(isMobile);
            
            Debug.Log($"[PlatformDetector] LevelSelector detected. LevelSelector UI: {isMobile}");
        }
        else if (IsGameplayLevelScene(currentSceneName))
        {
            // Niveles de gameplay: Activar controles de gameplay
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
        }
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
    
    // Método público para activar/desactivar manualmente (útil para casos especiales)
    public void SetMobileUIActive(bool gameplay, bool levelSelector)
    {
        SetGameplayUI(gameplay && IsMobile());
        SetLevelSelectorUI(levelSelector && IsMobile());
    }
}
