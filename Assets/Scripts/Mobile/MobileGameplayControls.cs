using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controles táctiles móviles serializados (joystick y botones de salto/pausa).
/// Se ocultan automáticamente cuando se detecta un mando físico conectado (USB-C o Bluetooth).
///
/// SOPORTA:
/// 1. Modo serializado (preferido): colocado en UI-Canvas con CanvasGroup y OnScreen controls.
/// 2. Modo fallback: genera los controles dinámicamente si la escena no tiene UI mobile serializada.
/// </summary>
public sealed class MobileGameplayControls : MonoBehaviour
{
    private const string ControlsObjectName = "Runtime Mobile Gameplay Controls";
    private const string GameplayScenePrefix = "Level_";
    private const float ReferenceWidth = 1920f;
    private static Sprite circleSprite;

    [Header("Referencias Serializadas (opcional, asignadas en Prefab)")]
    [SerializeField] private CanvasGroup controlsCanvasGroup;
    [SerializeField] private OnScreenStick onScreenStick;
    [SerializeField] private OnScreenButton jumpButton;
    [SerializeField] private OnScreenButton pauseButton;

    private GameObject runtimeControlsRoot;
    private bool isRuntimeCreated = false;

    public static bool IsMobile()
    {
#if UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return Application.isMobilePlatform;
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InputSystem.onDeviceChange += OnDeviceChanged;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsGameplayScene(scene.name))
            return;

        // Si ya hay un MobileGameplayControls serializado en la escena o prefab, no crear host runtime
        if (FindFirstObjectByType<MobileGameplayControls>() != null)
            return;

        var host = new GameObject("Mobile Gameplay Controls Controller");
        host.AddComponent<MobileGameplayControls>();
    }

    private static void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        if (!IsPhysicalGameController(device))
            return;

        var controllers = FindObjectsByType<MobileGameplayControls>(FindObjectsSortMode.None);
        foreach (var controller in controllers)
        {
            // OnScreenControl añade el dispositivo virtual antes de marcar uso OnScreen.
            // Diferir el check para no confundir el virtual con hardware.
            controller.CancelInvoke(nameof(RefreshVisibility));
            controller.Invoke(nameof(RefreshVisibility), 0.05f);
        }
    }

    private void Awake()
    {
        if (controlsCanvasGroup == null)
            controlsCanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!IsGameplayScene(scene.name))
        {
            if (isRuntimeCreated)
                Destroy(gameObject);
            return;
        }

        // Si no tiene CanvasGroup serializado, crear los controles en runtime como fallback
        if (controlsCanvasGroup == null)
        {
            CreateControls();
        }

        RefreshVisibility();
    }

    private void OnDestroy()
    {
        if (runtimeControlsRoot != null)
            Destroy(runtimeControlsRoot);
    }

    private void CreateControls()
    {
        isRuntimeCreated = true;
        runtimeControlsRoot = new GameObject(ControlsObjectName, typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));

        Canvas canvas = runtimeControlsRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = runtimeControlsRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        controlsCanvasGroup = runtimeControlsRoot.GetComponent<CanvasGroup>();
        controlsCanvasGroup.blocksRaycasts = true;
        controlsCanvasGroup.interactable = true;

        float responsiveScale = Mathf.Clamp(Screen.width / ReferenceWidth, 0.72f, 1.1f);
        float stickSize = 230f * responsiveScale;
        float buttonSize = 170f * responsiveScale;

        GameObject stickObject = CreateCircle("Move Stick", runtimeControlsRoot.transform,
            new Color(0.06f, 0.13f, 0.17f, 0.48f), stickSize, new Vector2(0f, 0f),
            new Vector2(150f, 145f));
        onScreenStick = stickObject.AddComponent<OnScreenStick>();
        onScreenStick.controlPath = "<Gamepad>/leftStick";
        onScreenStick.behaviour = OnScreenStick.Behaviour.RelativePositionWithStaticOrigin;
        onScreenStick.movementRange = stickSize * 0.32f;
        onScreenStick.useIsolatedInputActions = true;
        GameObject knob = CreateCircle("Stick Knob", stickObject.transform,
            new Color(0.85f, 0.94f, 0.92f, 0.62f), stickSize * 0.38f,
            new Vector2(0.5f, 0.5f), Vector2.zero);
        knob.GetComponent<Image>().raycastTarget = false;

        GameObject jumpObject = CreateCircle("Jump Button", runtimeControlsRoot.transform,
            new Color(0.88f, 0.25f, 0.17f, 0.72f), buttonSize,
            new Vector2(1f, 0f), new Vector2(-150f, 150f));
        jumpButton = jumpObject.AddComponent<OnScreenButton>();
        jumpButton.controlPath = "<Gamepad>/buttonSouth";

        Text label = CreateLabel("A", jumpObject.transform);
        label.fontSize = 58;
        label.color = Color.white;

        GameObject pauseObject = CreateCircle("Pause Button", runtimeControlsRoot.transform,
            new Color(0.06f, 0.13f, 0.17f, 0.5f), 92f * responsiveScale,
            new Vector2(0.5f, 1f), new Vector2(0f, -92f));
        pauseButton = pauseObject.AddComponent<OnScreenButton>();
        pauseButton.controlPath = "<Gamepad>/start";
        Text pauseLabel = CreateLabel("II", pauseObject.transform);
        pauseLabel.fontSize = 40;
        pauseLabel.color = Color.white;
    }

    private static GameObject CreateCircle(string name, Transform parent, Color color, float size,
        Vector2 anchor, Vector2 anchoredPosition)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = (RectTransform)gameObject.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(size, size);

        Image image = gameObject.GetComponent<Image>();
        image.sprite = GetCircleSprite();
        image.color = color;
        image.raycastTarget = true;
        return gameObject;
    }

    private static Text CreateLabel(string value, Transform parent)
    {
        var labelObject = new GameObject("Button Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = (RectTransform)labelObject.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text text = labelObject.GetComponent<Text>();
        text.text = value;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;
        return text;
    }

    public void RefreshVisibility()
    {
        if (controlsCanvasGroup == null)
            return;

        bool visible = IsMobile() && IsGameplayScene(SceneManager.GetActiveScene().name)
            && !HasPhysicalGameController();

        controlsCanvasGroup.alpha = visible ? 1f : 0f;
        controlsCanvasGroup.blocksRaycasts = visible;
        controlsCanvasGroup.interactable = visible;
    }

    public static bool HasPhysicalGameController()
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            if (IsPhysicalGameController(device) && device.enabled)
                return true;
        }
        return false;
    }

    public static bool IsPhysicalGameController(InputDevice device)
    {
        if (!(device is Gamepad) && !(device is Joystick))
            return false;

        // OnScreenStick/Button crean un Gamepad virtual con uso "OnScreen".
        InternedString onScreenUsage = new InternedString("OnScreen");
        foreach (InternedString usage in device.usages)
        {
            if (usage == onScreenUsage)
                return false;
        }
        return true;
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return sceneName.StartsWith(GameplayScenePrefix, System.StringComparison.OrdinalIgnoreCase);
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        const int resolution = 96;
        var texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
        {
            name = "Runtime Mobile Controls Circle",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Vector2 center = new Vector2((resolution - 1) * 0.5f, (resolution - 1) * 0.5f);
        float radius = resolution * 0.5f - 1f;
        var pixels = new Color32[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius + 0.75f - distance) * 255f);
                pixels[y * resolution + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        circleSprite = Sprite.Create(texture, new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f), resolution);
        circleSprite.name = "Runtime Mobile Controls Circle";
        return circleSprite;
    }
}
