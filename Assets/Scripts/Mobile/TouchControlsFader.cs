using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Keeps the on-screen controls faint while no finger is on the screen so they
/// do not hide the level, and brings them up while the player is touching.
/// </summary>
public class TouchControlsFader : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float idleAlpha = 0.18f;
    [SerializeField, Range(0f, 1f)] private float touchingAlpha = 0.6f;
    [SerializeField, Min(0.01f)] private float fadeInSeconds = 0.08f;
    [SerializeField, Min(0.01f)] private float fadeOutSeconds = 0.5f;
    [Tooltip("Controls to fade; their Image colors keep their RGB.")]
    [SerializeField] private List<Image> controls = new List<Image>();

    private float alpha;

    private void OnEnable()
    {
        alpha = idleAlpha;
        Apply();
    }

    private void Update()
    {
        bool touching = false;
        Touchscreen screen = Touchscreen.current;
        if (screen != null)
        {
            foreach (var touch in screen.touches)
                if (touch.press.isPressed) { touching = true; break; }
        }

        float target = touching ? touchingAlpha : idleAlpha;
        float speed = Mathf.Abs(touchingAlpha - idleAlpha) / (touching ? fadeInSeconds : fadeOutSeconds);
        float next = Mathf.MoveTowards(alpha, target, speed * Time.unscaledDeltaTime);
        if (Mathf.Approximately(next, alpha)) return;
        alpha = next;
        Apply();
    }

    private void Apply()
    {
        foreach (Image image in controls)
        {
            if (image == null) continue;
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }
}
