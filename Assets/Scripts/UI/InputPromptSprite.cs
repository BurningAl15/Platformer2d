using UnityEngine;

/// <summary>
/// Shows the input prompt for the device the player is using. The Animator
/// keeps animating the keyboard sprites; this swaps each frame for the touch
/// or gamepad equivalent after the Animator writes it.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class InputPromptSprite : MonoBehaviour
{
    [Tooltip("Keyboard frames the Animator uses, in order (for example SpaceBar_0, SpaceBar_1).")]
    [SerializeField] private Sprite[] keyboardFrames;
    [Tooltip("Touch frames matching keyboardFrames by index.")]
    [SerializeField] private Sprite[] touchFrames;
    [Tooltip("Gamepad frames matching keyboardFrames by index.")]
    [SerializeField] private Sprite[] gamepadFrames;

    private SpriteRenderer spriteRenderer;
    private Color keyboardTint;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // The keyboard art is white and tinted by the renderer; the button prompts carry their own colors.
        keyboardTint = spriteRenderer.color;
    }

    private void LateUpdate()
    {
        Sprite[] target = CurrentFrames();
        bool keyboard = target == keyboardFrames || target == null;
        spriteRenderer.color = keyboard ? keyboardTint : Color.white;
        if (keyboard || spriteRenderer.sprite == null) return;

        int index = System.Array.IndexOf(keyboardFrames, spriteRenderer.sprite);
        if (index >= 0 && index < target.Length && target[index] != null)
            spriteRenderer.sprite = target[index];
    }

    private Sprite[] CurrentFrames()
    {
        // A connected controller wins, then touch on mobile, then the keyboard.
        if (MobileGameplayControls.HasPhysicalGameController()) return gamepadFrames;
        if (MobileGameplayControls.IsMobile()) return touchFrames;
        return keyboardFrames;
    }
}
