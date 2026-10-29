using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FatCatStudio.Splash
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class FatCatStudioSplashController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float fadeInDuration = 0.35f;
        [SerializeField, Min(0f)] private float holdDuration = 1.3f;
        [SerializeField, Min(0f)] private float fadeOutDuration = 0.35f;

        private CanvasGroup canvasGroup;

        [SerializeField] private TMP_Text publisherTxt;
        [SerializeField] private TMP_Text companyTxt;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private RawImage logoImg;

        [SerializeField] private FatCatStudioSplashSettings.SplashStyle splashStyle;
        
        [SerializeField] private List<FatCatStudioSplashSettings> logoSprites = new List<FatCatStudioSplashSettings>();
        
        private FatCatStudioSplashSettings currentLogo;

        private void Awake()
        {
            TryInitializeTheme();
            ApplyCameraBackground();
        }

        private void ApplyCameraBackground()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = FindAnyObjectByType<Camera>();
            }

            if (cam != null && currentLogo != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = currentLogo.BackgroundColor;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (logoSprites != null && logoSprites.Count > 0)
            {
                FatCatStudioSplashSettings previewLogo = logoSprites.Find(logo => logo != null && logo.SplashStyleEnum == splashStyle);
                if (previewLogo != null)
                {
                    if (publisherTxt != null) publisherTxt.color = previewLogo.TextColor;
                    if (companyTxt != null) companyTxt.color = previewLogo.TextColor;
                    if (backgroundImg != null) backgroundImg.color = previewLogo.BackgroundColor;
                    if (logoImg != null) logoImg.texture = previewLogo.Logo;

                    Camera cam = Camera.main;
                    if (cam == null) cam = FindAnyObjectByType<Camera>();
                    if (cam != null)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = previewLogo.BackgroundColor;
                    }
                }
            }
        }
#endif

        private bool TryInitializeTheme()
        {
            if (currentLogo == null)
            {
                currentLogo = logoSprites.Find(logo => logo != null && logo.SplashStyleEnum == splashStyle);
            }

            if (currentLogo == null)
            {
                Debug.LogError($"[FatCatSplash] No settings asset was assigned for the {splashStyle} style.");
                return false;
            }

            if (publisherTxt == null || companyTxt == null || backgroundImg == null || logoImg == null)
            {
                Debug.LogError("[FatCatSplash] One or more UI references are missing in the splash scene.");
                return false;
            }

            publisherTxt.color = currentLogo.TextColor;
            companyTxt.color = currentLogo.TextColor;
            backgroundImg.color = currentLogo.BackgroundColor;
            logoImg.texture = currentLogo.Logo;
            ApplyCameraBackground();
            return true;
        }
        
        private IEnumerator Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            // A zeroed root scale makes the overlay effectively invisible and can be
            // accidentally serialized while editing the scene.
            transform.localScale = Vector3.one;

            if (!TryInitializeTheme())
            {
                LoadNextSceneImmediately();
                yield break;
            }

            // Keep the opaque background visible for the entire scene transition.
            // Only the studio mark and copy fade; fading the root CanvasGroup exposed
            // the camera/skybox before the first frame and after the logo disappeared.
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            SetContentAlpha(0f);

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError("[FatCatSplash] No enabled scene exists after the splash scene.");
                yield break;
            }

            yield return FadeContent(0f, 1f, fadeInDuration);
            yield return new WaitForSecondsRealtime(holdDuration);
            yield return FadeContent(1f, 0f, fadeOutDuration);

            // Keep the splash independent from the destination scene. Preloading here
            // makes a slow TitleScreen look like a frozen studio logo and can prevent
            // the splash animation from being shown at all.
            AsyncOperation load = SceneManager.LoadSceneAsync(nextSceneIndex);
            while (load != null && !load.isDone)
            {
                // Leave the theme background on screen while the destination loads.
                yield return null;
            }
        }

        private void LoadNextSceneImmediately()
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadSceneAsync(nextSceneIndex);
            }
        }

        private void SetContentAlpha(float alpha)
        {
            SetAlpha(publisherTxt, alpha);
            SetAlpha(companyTxt, alpha);
            SetAlpha(logoImg, alpha);
            if (backgroundImg != null)
            {
                Color background = backgroundImg.color;
                background.a = 1f;
                backgroundImg.color = background;
            }
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
                return;

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private IEnumerator FadeContent(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                SetContentAlpha(to);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetContentAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            SetContentAlpha(to);
        }
    }
}
