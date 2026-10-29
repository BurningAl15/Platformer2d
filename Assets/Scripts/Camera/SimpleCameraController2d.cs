using UnityEngine;

public class SimpleCameraController2d : MonoBehaviour
{
    public static SimpleCameraController2d _instance;
    
    [SerializeField] private Transform target;

    [Header("Parallax Layers")]
    [SerializeField] private Transform farBackground, middleBackground;
    private Vector2 lastPos;

    [Header("Camera Height Clamping")] 
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;

    private bool stopFollow = false;

    private float timer = 0;
    [SerializeField] private AnimationCurve _animationCurve;

    [SerializeField] private Camera cam;

    [Header("Background Coverage")]
    [Tooltip("Ensures the far sky covers the full camera width on wide mobile screens.")]
    [SerializeField, Min(1f)] private float backgroundWidthPadding = 2f;

    private static readonly Color SkyColor = new Color(0.2784f, 0.9529f, 1f, 1f); // #47F3FF
    private SpriteRenderer farBackgroundRenderer;
    private Vector2 lastScreenSize;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        if (target == null && PlayerController2d._instance != null)
            target = PlayerController2d._instance.transform;

        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam != null)
            cam.backgroundColor = SkyColor;

        if (farBackground != null)
        {
            farBackgroundRenderer = farBackground.GetComponentInChildren<SpriteRenderer>();
            // Centrar el fondo lejano horizontalmente con la cámara
            Vector3 farPos = farBackground.position;
            farPos.x = transform.position.x;
            farBackground.position = farPos;
        }

        lastPos = transform.position;
        UpdateFarBackgroundCoverage();
    }

    void Update()
    {
        if (target == null)
            return;

        if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
            UpdateFarBackgroundCoverage();

        if (!stopFollow)
        {
            //Horizontal Movement
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);

            //Clamping Vertical Camera
            float clampedY = Mathf.Clamp(transform.position.y, minHeight, maxHeight);
            transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);

            //Parallax - Horizontal (X) - Vertical (Y)
            Vector2 amountToMove = new Vector2(transform.position.x - lastPos.x,transform.position.y - lastPos.y);

            if (farBackground != null)
                farBackground.position += new Vector3(amountToMove.x, amountToMove.y, 0);
            if (middleBackground != null)
                middleBackground.position += new Vector3(amountToMove.x, amountToMove.y, 0) * .5f;
        
            lastPos = transform.position;
        }
        // else
        // {
        //     if (transform.position.y != target.position.y)
        //     {
        //         timer += Time.deltaTime;
        //         float val = _animationCurve.Evaluate(timer);
        //         float clampedY = Mathf.Lerp(transform.position.y, maxHeight,val);
        //         transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);
        //     }
        // }
    }

    private void UpdateFarBackgroundCoverage()
    {
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        if (cam == null || !cam.orthographic || farBackgroundRenderer == null || farBackgroundRenderer.sprite == null)
            return;

        // Asegurar que el fondo de la cámara sea idéntico al cielo (#47F3FF)
        cam.backgroundColor = SkyColor;

        // Cobertura completa del viewport horizontal para aspect ratios modernos (ej. 20:9)
        float requiredWidth = cam.orthographicSize * cam.aspect * 2f * Mathf.Max(backgroundWidthPadding, 1.5f);
        float spriteWidth = farBackgroundRenderer.sprite.bounds.size.x;
        if (spriteWidth <= 0f)
            return;

        Vector3 scale = farBackgroundRenderer.transform.localScale;
        float parentScaleX = farBackgroundRenderer.transform.parent != null
            ? farBackgroundRenderer.transform.parent.lossyScale.x
            : 1f;
        float currentWorldWidth = spriteWidth * Mathf.Abs(parentScaleX * scale.x);
        if (currentWorldWidth < requiredWidth)
        {
            scale.x = requiredWidth / (spriteWidth * Mathf.Max(0.0001f, Mathf.Abs(parentScaleX)));
            farBackgroundRenderer.transform.localScale = scale;
        }

        // Mantener el fondo lejano alineado al centro de la cámara
        if (farBackground != null)
        {
            Vector3 farPos = farBackground.position;
            farPos.x = transform.position.x;
            farBackground.position = farPos;
        }
    }

    public void StopFollow()
    {
        stopFollow = true;
    }

    public void Follow()
    {
        stopFollow = false;
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 temp = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        
        if (cam.orthographic) {
            float spread = cam.farClipPlane - cam.nearClipPlane;
            float center = (cam.farClipPlane + cam.nearClipPlane)*0.5f;
            Gizmos.color = new Color(0, 1, 0);
            Gizmos.DrawWireCube(new Vector3(transform.position.x,minHeight,transform.position.z), new Vector3(cam.orthographicSize*2*cam.aspect, cam.orthographicSize*2, spread));
            Gizmos.color = new Color(1, 0, 0);
            Gizmos.DrawWireCube(new Vector3(transform.position.x,maxHeight,transform.position.z), new Vector3(cam.orthographicSize*2*cam.aspect, cam.orthographicSize*2, spread));
        } else {
            Gizmos.DrawFrustum(new Vector3(transform.position.x,minHeight,transform.position.z), cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
            Gizmos.DrawFrustum(new Vector3(transform.position.x,maxHeight,transform.position.z), cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
        }
        Gizmos.matrix = temp;

    }
}
