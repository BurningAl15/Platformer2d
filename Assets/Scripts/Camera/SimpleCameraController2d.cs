using System;
using UnityEngine;
using DG.Tweening;

public class SimpleCameraController2d : MonoBehaviour
{
    public static SimpleCameraController2d _instance;
    
    [SerializeField] private Transform target;

    [Header("Parallax Layers")]
    [SerializeField] private Transform farBackground, middleBackground;
    [SerializeField] private Transform cloudBackground;
    [SerializeField] private Transform highCloudBackground;
    [SerializeField] private Transform seaShimmerBackground;
    [SerializeField, Range(0f, 1f)] private float cloudFollowFactor = 0.92f;
    [SerializeField, Range(0f, 1f)] private float highCloudFollowFactor = 0.96f;
    [SerializeField, Min(0f)] private float cloudDriftDistance = 0.35f;
    [SerializeField, Min(0.1f)] private float cloudDriftDuration = 12f;
    [SerializeField, Range(0f, 1f)] private float middleFollowFactor = 0.5f;
    [SerializeField, Range(0f, 1f)] private float seaShimmerMinAlpha = 0.9f;
    [SerializeField, Min(0.1f)] private float seaShimmerDuration = 5f;
    [SerializeField, Min(1f)] private float seaShimmerFrameRate = 8f;
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
    private SpriteRenderer cloudBackgroundRenderer;
    private SpriteRenderer highCloudBackgroundRenderer;
    private SpriteRenderer seaShimmerRenderer;
    private Sprite[] seaShimmerFrames;
    private Tween seaShimmerTween;
    private Tween cloudDriftTween;
    private Vector2 lastScreenSize;
    private float seaShimmerFrameTimer;
    private int currentSeaShimmerFrame;
    private float currentCloudDriftOffset;
    private float lastCloudDriftOffset;
    
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

        if (target != null)
        {
            Vector3 cameraPosition = transform.position;
            cameraPosition.x = target.position.x;
            cameraPosition.y = Mathf.Clamp(target.position.y, minHeight, maxHeight);
            transform.position = cameraPosition;
        }

        if (farBackground != null)
        {
            farBackgroundRenderer = farBackground.GetComponentInChildren<SpriteRenderer>();
            // Center the far background horizontally on the camera.
            Vector3 farPos = farBackground.position;
            farPos.x = transform.position.x;
            farBackground.position = farPos;
        }

        if (cloudBackground != null)
        {
            cloudBackgroundRenderer = cloudBackground.GetComponentInChildren<SpriteRenderer>();
            Vector3 cloudPos = cloudBackground.position;
            cloudPos.x = transform.position.x;
            if (farBackground != null)
                cloudPos.y = farBackground.position.y;
            cloudBackground.position = cloudPos;

            if (cloudDriftDistance > 0f && cloudDriftDuration > 0f)
            {
                cloudDriftTween = DOTween.To(() => currentCloudDriftOffset, value => currentCloudDriftOffset = value,
                        cloudDriftDistance, cloudDriftDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        if (highCloudBackground != null)
        {
            highCloudBackgroundRenderer = highCloudBackground.GetComponentInChildren<SpriteRenderer>();
            Vector3 highCloudPos = highCloudBackground.position;
            highCloudPos.x = cloudBackground != null ? cloudBackground.position.x : transform.position.x;
            highCloudPos.y = cloudBackground != null ? cloudBackground.position.y : highCloudPos.y;
            highCloudBackground.position = highCloudPos;
        }

        if (seaShimmerBackground != null)
        {
            seaShimmerRenderer = seaShimmerBackground.GetComponentInChildren<SpriteRenderer>();
            seaShimmerFrames = Resources.LoadAll<Sprite>("SeaWaves");
            Array.Sort(seaShimmerFrames, (left, right) => string.CompareOrdinal(left.name, right.name));

            if (seaShimmerRenderer != null && seaShimmerFrames.Length > 0)
                seaShimmerRenderer.sprite = seaShimmerFrames[0];

            Vector3 shimmerPos = seaShimmerBackground.position;
            shimmerPos.x = transform.position.x;
            if (farBackground != null)
                shimmerPos.y = farBackground.position.y;
            seaShimmerBackground.position = shimmerPos;

            if (seaShimmerRenderer != null)
            {
                seaShimmerTween = seaShimmerRenderer.DOFade(seaShimmerMinAlpha, seaShimmerDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        lastPos = transform.position;
        UpdateFarBackgroundCoverage();
    }

    private void OnDestroy()
    {
        seaShimmerTween?.Kill();
        cloudDriftTween?.Kill();
    }

    private void LateUpdate()
    {
        UpdateSeaShimmerAnimation();

        if (target == null)
        {
            ApplyCloudDrift();
            return;
        }

        if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
            UpdateFarBackgroundCoverage();

        if (!stopFollow)
        {
            // Horizontal movement.
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);

            // Clamp vertical camera movement.
            float clampedY = Mathf.Clamp(transform.position.y, minHeight, maxHeight);
            transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);

            // Apply camera-relative parallax after the camera reaches its final position.
            Vector2 amountToMove = new Vector2(transform.position.x - lastPos.x, transform.position.y - lastPos.y);

            if (farBackground != null)
                farBackground.position += new Vector3(amountToMove.x, amountToMove.y, 0);
            if (seaShimmerBackground != null)
                seaShimmerBackground.position += new Vector3(amountToMove.x, amountToMove.y, 0);
            if (cloudBackground != null)
                cloudBackground.position += new Vector3(amountToMove.x, amountToMove.y, 0) * cloudFollowFactor;
            if (highCloudBackground != null)
                highCloudBackground.position += new Vector3(amountToMove.x, amountToMove.y, 0) * highCloudFollowFactor;
            if (middleBackground != null)
                middleBackground.position += new Vector3(amountToMove.x, amountToMove.y, 0) * middleFollowFactor;
        
            lastPos = transform.position;
        }

        ApplyCloudDrift();
    }

    private void ApplyCloudDrift()
    {
        if (cloudBackground == null)
            return;

        float driftDelta = currentCloudDriftOffset - lastCloudDriftOffset;
        cloudBackground.position += Vector3.right * driftDelta;
        lastCloudDriftOffset = currentCloudDriftOffset;
    }

    private void UpdateFarBackgroundCoverage()
    {
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        if (cam == null || !cam.orthographic || farBackgroundRenderer == null || farBackgroundRenderer.sprite == null)
            return;

        // Match the camera clear color to the sky (#47F3FF).
        cam.backgroundColor = SkyColor;

        // Cover the horizontal viewport on modern aspect ratios (for example, 20:9).
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

        if (cloudBackgroundRenderer != null && farBackgroundRenderer != null)
        {
            Vector3 cloudScale = cloudBackground.localScale;
            cloudScale.x = farBackgroundRenderer.transform.localScale.x;
            cloudBackground.localScale = cloudScale;
        }

        if (highCloudBackgroundRenderer != null && farBackgroundRenderer != null)
        {
            Vector3 highCloudScale = highCloudBackground.localScale;
            highCloudScale.x = farBackgroundRenderer.transform.localScale.x;
            highCloudBackground.localScale = highCloudScale;
        }

        if (seaShimmerRenderer != null && farBackgroundRenderer != null)
        {
            Vector3 shimmerScale = seaShimmerBackground.localScale;
            shimmerScale.x = farBackgroundRenderer.transform.localScale.x;
            seaShimmerBackground.localScale = shimmerScale;
        }

        // Keep the far background aligned with the camera center.
        if (farBackground != null)
        {
            Vector3 farPos = farBackground.position;
            farPos.x = transform.position.x;
            farBackground.position = farPos;
        }
    }

    private void UpdateSeaShimmerAnimation()
    {
        if (seaShimmerRenderer == null || seaShimmerFrames == null || seaShimmerFrames.Length < 2)
            return;

        seaShimmerFrameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, seaShimmerFrameRate);
        while (seaShimmerFrameTimer >= frameDuration)
        {
            seaShimmerFrameTimer -= frameDuration;
            currentSeaShimmerFrame = (currentSeaShimmerFrame + 1) % seaShimmerFrames.Length;

            seaShimmerRenderer.sprite = seaShimmerFrames[currentSeaShimmerFrame];
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
