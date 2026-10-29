using System.IO;
using System.Linq;
using FatCatStudio.Splash;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FatCatStudio.Splash.Editor
{
    public static class FatCatStudioSplashTools
    {
        private const string RootFolder = "Assets/FatCatStudio/Splash";
        private const string ScenePath = RootFolder + "/FatCatSplash.unity";
        private const string LogoPath = RootFolder + "/Art/fatcat-stacked-light.jpg";
        private const string PackageOutput = "../../SharedPackages/FatCatStudioSplash.unitypackage";

        [MenuItem("Tools/Fat Cat Studio/Splash/Create or Refresh Scene")]
        public static void CreateOrRefreshScene()
        {
            Texture2D logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoPath);
            if (logoTexture == null)
            {
                throw new FileNotFoundException("Fat Cat splash logo was not found.", LogoPath);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.19607843f, 0.19607843f, 0.19607843f, 1f);
            camera.tag = "MainCamera";

            GameObject canvasObject = new GameObject(
                "FatCatSplash",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(CanvasGroup),
                typeof(FatCatStudioSplashController));

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.localScale = Vector3.one;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Image background = CreateImage("Background", canvasObject.transform);
            Stretch(background.rectTransform);
            background.color = Color.white;

            Text publisher = CreateText("Publisher", canvasObject.transform, "PUBLISHED BY", 30, FontStyle.Normal);
            SetCenteredRect(publisher.rectTransform, new Vector2(760f, 70f), new Vector2(0f, 430f));
            publisher.color = new Color32(74, 74, 74, 255);

            GameObject logoObject = new GameObject("Fat Cat Game Studio Logo", typeof(RectTransform), typeof(RawImage));
            logoObject.transform.SetParent(canvasObject.transform, false);
            RawImage logo = logoObject.GetComponent<RawImage>();
            logo.texture = logoTexture;
            logo.color = Color.white;
            SetCenteredRect(logo.rectTransform, new Vector2(780f, 780f), Vector2.zero);

            Text company = CreateText("Company", canvasObject.transform, "A L\u00c1N STUDIO COMPANY", 28, FontStyle.Normal);
            SetCenteredRect(company.rectTransform, new Vector2(840f, 70f), new Vector2(0f, -440f));
            company.color = new Color32(86, 86, 86, 255);

            EditorSceneManager.SaveScene(scene, ScenePath);
            PutSplashFirstInBuildSettings();
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = canvasObject;

            Debug.Log($"[FatCatSplash] Scene created and installed: {ScenePath}");
        }

        [MenuItem("Tools/Fat Cat Studio/Splash/Put First in Build Settings")]
        public static void PutSplashFirstInBuildSettings()
        {
            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
            EditorBuildSettingsScene splashScene = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = new[] { splashScene }
                .Concat(currentScenes.Where(scene => scene.path != ScenePath && scene.path != "Assets/Scenes/SplashScreen.unity"))
                .ToArray();

            Debug.Log("[FatCatSplash] Splash scene is now the first enabled build scene.");
        }

        [MenuItem("Tools/Fat Cat Studio/Splash/Export Unity Package")]
        public static void ExportPackage()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputPath = Path.GetFullPath(Path.Combine(projectRoot, PackageOutput));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
#pragma warning disable CS0618
            AssetDatabase.ExportPackage(RootFolder, outputPath, ExportPackageOptions.Recurse);
#pragma warning restore CS0618
            Debug.Log($"[FatCatSplash] Package exported: {outputPath}");
            if (!Application.isBatchMode)
                EditorUtility.RevealInFinder(outputPath);
        }

        private static Image CreateImage(string name, Transform parent)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            return imageObject.GetComponent<Image>();
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, FontStyle style)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void SetCenteredRect(RectTransform rectTransform, Vector2 size, Vector2 position)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;
        }
    }
}
