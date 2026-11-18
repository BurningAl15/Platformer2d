using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Intro narration shown before the tutorial on New Game: one layered pixel-art
/// illustration with a gentle parallax drift, and a narration box whose pages
/// type out letter by letter. A tap, click, Space/Enter or the gamepad South
/// button finishes the current page or advances; Skip goes straight to the
/// tutorial.
/// </summary>
public class StoryController : MonoBehaviour
{
    [System.Serializable]
    private struct ParallaxLayer
    {
        public Transform layer;
        [Tooltip("World units the layer drifts over one sway cycle.")]
        public float drift;
    }

    [Header("Illustration")]
    [SerializeField] private Camera storyCamera;
    [SerializeField] private SpriteRenderer framingSprite;
    [Tooltip("Extra zoom so drifting layers never reveal an edge.")]
    [SerializeField] private float overscan = 1.06f;
    [SerializeField] private ParallaxLayer[] layers;
    [SerializeField] private float swaySeconds = 14f;

    [Header("Narration")]
    [SerializeField] private string[] pageKeys = { "STORY_1", "STORY_2", "STORY_3", "STORY_4" };
    [SerializeField] private TextMeshProUGUI narration;
    [SerializeField] private GameObject continueHint;
    [SerializeField] private Button skipButton;
    [SerializeField] private TextMeshProUGUI skipLabel;
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private float charactersPerSecond = 38f;

    private int page = -1;
    private bool typing;
    private bool leaving;
    private Coroutine typeRoutine;
    private Vector3[] homes;

    private void Start()
    {
        FitCamera();
        homes = new Vector3[layers.Length];
        for (int i = 0; i < layers.Length; i++)
            if (layers[i].layer != null) homes[i] = layers[i].layer.localPosition;

        if (skipButton != null) skipButton.onClick.AddListener(Finish);
        if (skipLabel != null) skipLabel.text = GameLocalization.Get("STORY_SKIP");
        if (fade != null)
        {
            fade.alpha = 1f;
            fade.DOFade(0f, 0.8f).SetUpdate(true).SetLink(gameObject);
        }
        NextPage();
    }

    private void Update()
    {
        if (leaving) return;
        Sway();
        if (AdvancePressed()) Advance();
    }

    /// <summary>Cover the screen with the illustration at any aspect ratio.</summary>
    private void FitCamera()
    {
        if (storyCamera == null || framingSprite == null) return;
        Vector2 size = framingSprite.sprite.bounds.size;
        float byHeight = size.y / 2f;
        float byWidth = size.x / (2f * storyCamera.aspect);
        storyCamera.orthographicSize = Mathf.Min(byHeight, byWidth) / overscan;
    }

    private void Sway()
    {
        float t = Mathf.Sin(Time.unscaledTime / swaySeconds * Mathf.PI * 2f);
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layer == null) continue;
            layers[i].layer.localPosition = homes[i] + new Vector3(t * layers[i].drift, 0f, 0f);
        }
    }

    private bool AdvancePressed()
    {
        Vector2? press = null;
        Touchscreen screen = Touchscreen.current;
        if (screen != null)
            foreach (var touch in screen.touches)
                if (touch.press.wasPressedThisFrame) { press = touch.position.ReadValue(); break; }

        Mouse mouse = Mouse.current;
        if (press == null && mouse != null && mouse.leftButton.wasPressedThisFrame)
            press = mouse.position.ReadValue();

        // Taps on the Skip button belong to the button.
        if (press.HasValue)
            return !UISelectionUtility.IsScreenPointOverUI(press.Value);

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
            return true;
        Gamepad pad = Gamepad.current;
        return pad != null && pad.buttonSouth.wasPressedThisFrame;
    }

    private void Advance()
    {
        if (typing)
        {
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            narration.maxVisibleCharacters = int.MaxValue;
            EndTyping();
            return;
        }
        NextPage();
    }

    private void NextPage()
    {
        page++;
        if (page >= pageKeys.Length)
        {
            Finish();
            return;
        }
        narration.text = GameLocalization.Get(pageKeys[page]);
        typeRoutine = StartCoroutine(TypePage());
    }

    private IEnumerator TypePage()
    {
        typing = true;
        if (continueHint != null) continueHint.SetActive(false);
        narration.maxVisibleCharacters = 0;
        narration.ForceMeshUpdate();
        int total = narration.textInfo.characterCount;
        float shown = 0f;
        while (shown < total)
        {
            shown += charactersPerSecond * Time.unscaledDeltaTime;
            narration.maxVisibleCharacters = Mathf.FloorToInt(shown);
            yield return null;
        }
        EndTyping();
    }

    private void EndTyping()
    {
        typing = false;
        typeRoutine = null;
        if (continueHint != null) continueHint.SetActive(true);
    }

    private void Finish()
    {
        if (leaving) return;
        leaving = true;
        if (skipButton != null) skipButton.interactable = false;
        if (fade == null)
        {
            SceneUtils.LoadGameplayScene(0);
            return;
        }
        fade.blocksRaycasts = true;
        fade.DOFade(1f, 0.6f).SetUpdate(true).SetLink(gameObject)
            .OnComplete(() => SceneUtils.LoadGameplayScene(0));
    }
}
