using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Script de Editor para configurar automáticamente todos los elementos faltantes,
/// prefabs y paneles de UI en Fox's Tale con el estilo pixel art nativo del juego.
/// 
/// SE EJECUTA DESDE: Menú -> CustomEditorTools -> Setup Missing Scene Elements & Placeholders
/// o automáticamente al compilar el script una vez.
/// </summary>
[InitializeOnLoad]
public static class SceneElementSetup
{
    private const string SETUP_FLAG_KEY = "FoxsTale_PixelUISetup_Completed_v2";

    static SceneElementSetup()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool(SETUP_FLAG_KEY, false))
            {
                SessionState.SetBool(SETUP_FLAG_KEY, true);
                SetupAll();
            }
        };
    }

    [MenuItem("CustomEditorTools/Setup Missing Scene Elements & Placeholders")]
    public static void SetupAll()
    {
        Debug.Log("[SceneElementSetup] Iniciando configuracion de escenas y paneles pixel-art...");

        try
        {
            AssetDatabase.StartAssetEditing();

            // 1. Crear prefabs de coleccionables
            GameObject trebolPrefab = CreateTrebolPrefab();
            GameObject diceFragPrefab = CreateDiceFragmentPrefab();

            // 2. Configurar MainMenu (con Store-Button pixel art y StorePanel al frente)
            SetupMainMenuScene();

            // 3. Configurar LevelSelection (badges y responsive bottom panel)
            SetupLevelSelectionScene();

            // 4. Configurar UI-Canvas Prefab (GameOverPanel, DiceRollPanel, StorePanel con estilo pixel art)
            SetupUICanvasPrefab();

            // 5. Agregar Trebol a cada nivel de gameplay
            SetupGameplayLevels(trebolPrefab);

            // 6. Revisión exhaustiva de auto-sizing en todos los textos de prefabs y escenas
            ConfigureAllProjectTextsForAutoSizing();

            Debug.Log("[SceneElementSetup] Todas las escenas y paneles configurados con exito en estilo pixel art y auto-sizing!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SceneElementSetup] Error durante la configuración: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Recursos Visuales Pixel Art del Proyecto
    // ─────────────────────────────────────────────────────────────────────────

    private static TMP_FontAsset GetPixelFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/2D Platformer Assets/Font/Pixelnauts SDF 1.asset");
        if (font == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset Pixelnauts");
            if (guids.Length > 0)
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        return font;
    }

    private static Sprite GetPanelSprite()
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D Platformer Assets/Graphics/UI/UI_Panel.png");
        if (s == null)
            s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D Platformer Assets/Graphics/UI/Pause.png");
        return s;
    }

    private static (Sprite normal, Sprite highlighted, Sprite pressed, Sprite warning) GetButtonSprites()
    {
        Sprite normal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D Platformer Assets/Graphics/UI/UI_Buttons_1_Normal.png");
        Sprite highlighted = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D Platformer Assets/Graphics/UI/UI_Buttons_2_Highlighted.png");
        Sprite pressed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D Platformer Assets/Graphics/UI/UI_Buttons_3_Pressed.png");
        Sprite warning = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D Platformer Assets/Graphics/UI/UI_Buttons_3_Warning.png");
        return (normal, highlighted, pressed, warning);
    }

    private static Sprite[] GetDiceFaceSprites()
    {
        Sprite[] sprites = new Sprite[6];
        for (int i = 1; i <= 6; i++)
        {
            sprites[i - 1] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/2D Platformer Assets/Graphics/UI/Dice/dice_face_{i}.png");
        }
        return sprites;
    }

    private static (GameObject root, GameObject container, CanvasGroup cg, RectTransform containerRect) CreatePixelPanelRoot(
        GameObject canvasRoot, string name, Vector2 containerSize)
    {
        GameObject panelObj = new GameObject(name);
        panelObj.transform.SetParent(canvasRoot.transform, false);

        var rootRect = panelObj.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        var cg = panelObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // Backdrop oscuro semitransparente que cubre y bloquea toda la pantalla
        var backdrop = panelObj.AddComponent<Image>();
        backdrop.color = new Color(0.02f, 0.04f, 0.08f, 0.82f);

        // Contenedor centrado con marco pixel art 9-slice
        GameObject container = new GameObject("Container");
        container.transform.SetParent(panelObj.transform, false);

        var cRect = container.AddComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = containerSize;
        cRect.anchoredPosition = Vector2.zero;

        var cImg = container.AddComponent<Image>();
        cImg.sprite = GetPanelSprite();
        cImg.type = Image.Type.Sliced;
        cImg.color = new Color(1f, 1f, 1f, 0.98f);

        return (panelObj, container, cg, cRect);
    }

    private static Button CreatePixelButton(GameObject parent, string name, string label, Vector2 anchoredPos, Vector2 size, bool isWarning = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;

        var (normal, highlighted, pressed, warning) = GetButtonSprites();

        var img = go.AddComponent<Image>();
        img.sprite = isWarning ? (warning ?? normal) : normal;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.SpriteSwap;

        SpriteState state = new SpriteState
        {
            highlightedSprite = isWarning ? warning : highlighted,
            pressedSprite = pressed,
            selectedSprite = isWarning ? warning : highlighted
        };
        btn.spriteState = state;

        GameObject textObj = new GameObject("Text (TMP)");
        textObj.transform.SetParent(go.transform, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetPixelFont();
        tmp.text = label;
        float baseSize = size.y > 65 ? 26f : 22f;
        tmp.fontSize = baseSize;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 14f;
        tmp.fontSizeMax = baseSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Ellipsis;

        return btn;
    }

    private static GameObject CreatePixelText(GameObject parent, string name, string text, float fontSize, Vector2 anchoredPos, Vector2 size, Color? color = null, bool allowWrapping = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = GetPixelFont();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = Mathf.Min(14f, fontSize * 0.5f);
        tmp.fontSizeMax = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color ?? Color.white;
        tmp.textWrappingMode = allowWrapping ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Ellipsis;

        return go;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  1. Prefabs de coleccionables
    // ─────────────────────────────────────────────────────────────────────────

    private static GameObject CreateTrebolPrefab()
    {
        string path = "Assets/Prefabs/TrebolPickup.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject go = new GameObject("TrebolPickup");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.2f, 0.85f, 0.3f, 1f);
        sr.sortingLayerName = "Interactables";

        Sprite defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D Platformer Assets/Items/Gems/gem-1.png");
        if (defaultSprite == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite Gem");
            if (guids.Length > 0)
                defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        if (defaultSprite != null) sr.sprite = defaultSprite;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        go.AddComponent<TrebolPickup>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        GameObject.DestroyImmediate(go);
        Debug.Log($"[SceneElementSetup] Prefab creado: {path}");
        return prefab;
    }

    private static GameObject CreateDiceFragmentPrefab()
    {
        string path = "Assets/Prefabs/DiceFragmentPickup.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject go = new GameObject("DiceFragmentPickup");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.84f, 0f, 1f);
        sr.sortingLayerName = "Interactables";

        Sprite defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/2D Platformer Assets/Items/Gems/gem-1.png");
        if (defaultSprite != null) sr.sprite = defaultSprite;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        go.AddComponent<DiceFragmentPickup>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        GameObject.DestroyImmediate(go);
        Debug.Log($"[SceneElementSetup] Prefab creado: {path}");
        return prefab;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  2. Escena MainMenu
    // ─────────────────────────────────────────────────────────────────────────

    private static void SetupMainMenuScene()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        if (!File.Exists(scenePath)) return;

        Scene currentScene = EditorSceneManager.GetActiveScene();
        bool wasDifferentScene = currentScene.path != scenePath;

        Scene scene = wasDifferentScene 
            ? EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)
            : currentScene;

        bool modified = false;

        // AppBootstrap
        if (Object.FindObjectOfType<AppBootstrap>() == null)
        {
            GameObject bootObj = new GameObject("AppBootstrap");
            bootObj.AddComponent<AppBootstrap>();
            Undo.RegisterCreatedObjectUndo(bootObj, "Create AppBootstrap");
            modified = true;
            Debug.Log("[SceneElementSetup] AppBootstrap creado en MainMenu.");
        }

        // SceneTransitionManager
        if (Object.FindObjectOfType<SceneTransitionManager>() == null)
        {
            GameObject transObj = new GameObject("SceneTransitionManager");
            transObj.AddComponent<SceneTransitionManager>();
            Undo.RegisterCreatedObjectUndo(transObj, "Create SceneTransitionManager");
            modified = true;
            Debug.Log("[SceneElementSetup] SceneTransitionManager creado en MainMenu.");
        }

        MainMenuManager mmm = Object.FindObjectOfType<MainMenuManager>();
        Canvas canvas = mmm != null ? mmm.GetComponent<Canvas>() : null;
        if (canvas == null)
        {
            foreach (var c in Object.FindObjectsOfType<Canvas>())
            {
                if (c.transform.Find("Text-Img") != null || c.GetComponent<MainMenuManager>() != null)
                {
                    canvas = c;
                    break;
                }
            }
        }
        if (canvas == null)
            canvas = Object.FindObjectOfType<Canvas>();

        if (canvas != null)
        {
            // 1. Localizar el contenedor de botones de menu (ButtonMenu)
            Transform buttonMenu = canvas.transform.Find("ButtonMenu");
            if (buttonMenu == null)
            {
                foreach (Transform child in canvas.transform)
                {
                    if (child.name == "StorePanel") continue;
                    Button[] btns = child.GetComponentsInChildren<Button>(true);
                    if (btns.Length > 0)
                    {
                        buttonMenu = child;
                        break;
                    }
                }
            }

            if (buttonMenu != null)
            {
                buttonMenu.gameObject.name = "ButtonMenu";
            }

            // 2. Recrear StorePanel al frente del Canvas principal (y eliminar de cualquier otro canvas como BG-Canvas)
            foreach (var c in Object.FindObjectsOfType<Canvas>(true))
            {
                Transform sp = c.transform.Find("StorePanel");
                if (sp != null)
                {
                    Object.DestroyImmediate(sp.gameObject);
                    modified = true;
                }
            }
            CreateStorePanel(canvas.gameObject, false);
            Transform storePanelObj = canvas.transform.Find("StorePanel");
            if (storePanelObj != null)
            {
                storePanelObj.SetAsLastSibling();
                storePanelObj.gameObject.SetActive(false);
                modified = true;
            }

            // 3. Crear o actualizar Store-Button en el menu con el estilo visual pixel-art idéntico
            if (buttonMenu != null)
            {
                RectTransform bmRect = buttonMenu.GetComponent<RectTransform>();
                if (bmRect != null)
                {
                    bmRect.sizeDelta = new Vector2(400f, 480f);
                }

                Transform continueBtn = buttonMenu.Find("Continue-Button");
                Transform startBtn = buttonMenu.Find("Start-Button");
                Transform quitBtn = buttonMenu.Find("Quit-Button");
                Transform storeBtn = buttonMenu.Find("Store-Button");

                if (storeBtn != null)
                {
                    Object.DestroyImmediate(storeBtn.gameObject);
                }

                Button newStoreBtn = CreatePixelButton(buttonMenu.gameObject, "Store-Button", "Remove Ads", Vector2.zero, new Vector2(400f, 85f));
                storeBtn = newStoreBtn.transform;
                modified = true;

                // Ordenar jerarquia de botones
                if (continueBtn != null) continueBtn.SetSiblingIndex(0);
                if (startBtn != null) startBtn.SetSiblingIndex(1);
                storeBtn.SetSiblingIndex(2);
                if (quitBtn != null) quitBtn.SetSiblingIndex(3);

                // Posicionar armonicamente de arriba a abajo
                if (continueBtn != null)
                {
                    RectTransform crt = continueBtn.GetComponent<RectTransform>();
                    crt.anchorMin = new Vector2(0.5f, 0.5f);
                    crt.anchorMax = new Vector2(0.5f, 0.5f);
                    crt.pivot = new Vector2(0.5f, 0.5f);
                    crt.anchoredPosition = new Vector2(0f, 150f);
                    crt.sizeDelta = new Vector2(400f, 85f);
                }
                if (startBtn != null)
                {
                    RectTransform srt = startBtn.GetComponent<RectTransform>();
                    srt.anchorMin = new Vector2(0.5f, 0.5f);
                    srt.anchorMax = new Vector2(0.5f, 0.5f);
                    srt.pivot = new Vector2(0.5f, 0.5f);
                    srt.anchoredPosition = new Vector2(0f, 50f);
                    srt.sizeDelta = new Vector2(400f, 85f);
                }
                if (storeBtn != null)
                {
                    RectTransform strt = storeBtn.GetComponent<RectTransform>();
                    strt.anchorMin = new Vector2(0.5f, 0.5f);
                    strt.anchorMax = new Vector2(0.5f, 0.5f);
                    strt.pivot = new Vector2(0.5f, 0.5f);
                    strt.anchoredPosition = new Vector2(0f, -50f);
                    strt.sizeDelta = new Vector2(400f, 85f);
                }
                if (quitBtn != null)
                {
                    RectTransform qrt = quitBtn.GetComponent<RectTransform>();
                    qrt.anchorMin = new Vector2(0.5f, 0.5f);
                    qrt.anchorMax = new Vector2(0.5f, 0.5f);
                    qrt.pivot = new Vector2(0.5f, 0.5f);
                    qrt.anchoredPosition = new Vector2(0f, -150f);
                    qrt.sizeDelta = new Vector2(400f, 85f);
                }

                // Enlazar Store-Button con StorePanel tanto via SerializedProperty como persistent listener
                if (storePanelObj != null)
                {
                    NoAdsPurchaseButton noAdsScript = storePanelObj.GetComponent<NoAdsPurchaseButton>();
                    if (noAdsScript != null)
                    {
                        SerializedObject soNoAds = new SerializedObject(noAdsScript);
                        SerializedProperty openProp = soNoAds.FindProperty("openStoreButton");
                        if (openProp != null)
                        {
                            openProp.objectReferenceValue = newStoreBtn;
                            soNoAds.ApplyModifiedProperties();
                        }
                        UnityEditor.Events.UnityEventTools.AddPersistentListener(newStoreBtn.onClick, noAdsScript.OpenPanel);
                    }
                }
                Debug.Log("[SceneElementSetup] Store-Button creado y posicionado armónicamente en ButtonMenu.");
            }
            else
            {
                Debug.LogWarning("[SceneElementSetup] No se encontro el contenedor ButtonMenu en Canvas.");
            }
        }

        if (modified)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SceneElementSetup] MainMenu configurado con Store-Button pixel art y StorePanel al frente.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  3. Escena LevelSelection
    // ─────────────────────────────────────────────────────────────────────────

    private static void SetupLevelSelectionScene()
    {
        string scenePath = "Assets/Scenes/LevelSelection.unity";
        if (!File.Exists(scenePath)) return;

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        bool modified = false;

        MapPoint[] mapPoints = Object.FindObjectsOfType<MapPoint>();
        foreach (var point in mapPoints)
        {
            SerializedObject so = new SerializedObject(point);
            SerializedProperty trebolProp = so.FindProperty("trebolBadge");

            if (trebolProp != null && trebolProp.objectReferenceValue == null)
            {
                Transform existingBadge = point.transform.Find("Badge_Trebol");
                GameObject badgeObj = null;

                if (existingBadge != null)
                {
                    badgeObj = existingBadge.gameObject;
                }
                else
                {
                    badgeObj = new GameObject("Badge_Trebol");
                    badgeObj.transform.SetParent(point.transform, false);
                    badgeObj.transform.localPosition = new Vector3(0.45f, 0.45f, 0f);

                    var sr = badgeObj.AddComponent<SpriteRenderer>();
                    sr.color = new Color(0.2f, 0.9f, 0.3f, 1f);
                    sr.sortingLayerName = "UI";

                    Transform gemBadge = point.transform.Find("Badge_Gem (1)") ?? point.transform.Find("Badge_Gem");
                    if (gemBadge != null)
                    {
                        var gemSr = gemBadge.GetComponent<SpriteRenderer>();
                        if (gemSr != null && gemSr.sprite != null)
                            sr.sprite = gemSr.sprite;
                    }
                    badgeObj.SetActive(false);
                }

                trebolProp.objectReferenceValue = badgeObj;
                so.ApplyModifiedProperties();
                modified = true;
            }
        }

        UI_SelectController uiSelect = Object.FindObjectOfType<UI_SelectController>();
        if (uiSelect != null)
        {
            SerializedObject soSelect = new SerializedObject(uiSelect);
            soSelect.FindProperty("visibleY").floatValue = 30f;
            soSelect.FindProperty("hiddenY").floatValue = -350f;
            soSelect.ApplyModifiedProperties();

            // Configurar anclaje responsive infalible para iPhone SE y pantallas 16:9
            SerializedProperty containerProp = soSelect.FindProperty("levelInfoContainer");
            if (containerProp != null && containerProp.objectReferenceValue != null)
            {
                GameObject containerObj = (GameObject)containerProp.objectReferenceValue;
                RectTransform rt = containerObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.pivot = new Vector2(0.5f, 0f);
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.sizeDelta = new Vector2(-200f, 270f);
                    rt.anchoredPosition = new Vector2(0f, -350f);
                }
            }
            modified = true;
        }

        if (modified)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[SceneElementSetup] LevelSelection configurado responsive y con trebol badges.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  4. Prefab UI-Canvas
    // ─────────────────────────────────────────────────────────────────────────

    private static void SetupUICanvasPrefab()
    {
        string prefabPath = "Assets/Prefabs/ReUsable/UI-Canvas.prefab";
        if (!File.Exists(prefabPath)) return;

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        if (root == null) return;

        UIController uiController = root.GetComponent<UIController>();
        if (uiController == null)
        {
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        SerializedObject soUI = new SerializedObject(uiController);
        TMP_FontAsset pixelFont = GetPixelFont();

        // 1. Configurar HUD Texts (Trebol y Vidas)
        Transform hudContainer = root.transform.Find("Gems_Txt")?.parent ?? root.transform;

        // Trebol HUD Text
        Transform trebolTxtTransform = root.transform.Find("Trebol_Txt");
        if (trebolTxtTransform == null)
        {
            GameObject trebolTxtObj = new GameObject("Trebol_Txt");
            trebolTxtObj.transform.SetParent(hudContainer, false);
            var rect = trebolTxtObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.82f, 0.88f);
            rect.anchorMax = new Vector2(0.98f, 0.98f);
            rect.anchoredPosition = Vector2.zero;

            var tmp = trebolTxtObj.AddComponent<TextMeshProUGUI>();
            if (pixelFont != null) tmp.font = pixelFont;
            tmp.text = "x0";
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Right;

            SerializedProperty trebolTextProp = soUI.FindProperty("trebolCountText");
            if (trebolTextProp != null) trebolTextProp.objectReferenceValue = tmp;
        }
        else
        {
            var tmp = trebolTxtTransform.GetComponent<TextMeshProUGUI>();
            if (tmp != null && pixelFont != null) tmp.font = pixelFont;
        }

        // Vidas HUD Text
        Transform livesTxtTransform = root.transform.Find("Lives_Txt");
        if (livesTxtTransform == null)
        {
            GameObject livesTxtObj = new GameObject("Lives_Txt");
            livesTxtObj.transform.SetParent(hudContainer, false);
            var rect = livesTxtObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.85f);
            rect.anchorMax = new Vector2(0.18f, 0.95f);
            rect.anchoredPosition = Vector2.zero;

            var tmp = livesTxtObj.AddComponent<TextMeshProUGUI>();
            if (pixelFont != null) tmp.font = pixelFont;
            tmp.text = "x3";
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Left;

            SerializedProperty livesTextProp = soUI.FindProperty("livesText");
            if (livesTextProp != null) livesTextProp.objectReferenceValue = tmp;
        }
        else
        {
            var tmp = livesTxtTransform.GetComponent<TextMeshProUGUI>();
            if (tmp != null && pixelFont != null) tmp.font = pixelFont;
        }

        // Gems HUD Text (resetear de 999 a 0 y configurar NoWrap con auto-sizing)
        Transform gemsTxtTransform = root.transform.Find("GemsContainer/Gems_Txt") ?? root.transform.Find("Gems_Txt");
        if (gemsTxtTransform != null)
        {
            var tmpGems = gemsTxtTransform.GetComponent<TextMeshProUGUI>();
            if (tmpGems != null)
            {
                if (pixelFont != null) tmpGems.font = pixelFont;
                tmpGems.text = "0";
                tmpGems.textWrappingMode = TextWrappingModes.NoWrap;
                tmpGems.overflowMode = TextOverflowModes.Overflow;
                tmpGems.enableAutoSizing = true;
                tmpGems.fontSizeMin = 36;
                tmpGems.fontSizeMax = 90;
                RectTransform rt = gemsTxtTransform.GetComponent<RectTransform>();
                if (rt != null) rt.sizeDelta = new Vector2(300f, rt.sizeDelta.y);
                SerializedProperty gemsLevelProp = soUI.FindProperty("gemsLevelText");
                if (gemsLevelProp != null) gemsLevelProp.objectReferenceValue = tmpGems;
            }
        }

        soUI.ApplyModifiedProperties();

        // 2. MobileControls: NO DEBEN EXISTIR EN UI-Canvas.prefab.
        Transform existingMobileControls = root.transform.Find("MobileControls");
        if (existingMobileControls != null)
        {
            Object.DestroyImmediate(existingMobileControls.gameObject);
            Debug.Log("[SceneElementSetup] MobileControls duplicado eliminado de UI-Canvas.prefab.");
        }

        // 3. Recrear GameOverPanel con estilo pixel art nativo
        Transform existingGOPanel = root.transform.Find("GameOverPanel");
        if (existingGOPanel != null)
        {
            Object.DestroyImmediate(existingGOPanel.gameObject);
        }
        CreateGameOverPanel(root);

        // 4. Recrear DiceRollPanel con estilo pixel art nativo y caras del dado gráficas
        Transform existingDicePanel = root.transform.Find("DiceRollPanel");
        if (existingDicePanel != null)
        {
            Object.DestroyImmediate(existingDicePanel.gameObject);
        }
        CreateDiceRollPanel(root);

        // 5. Recrear StorePanel con estilo pixel art nativo
        Transform existingStorePanel = root.transform.Find("StorePanel");
        if (existingStorePanel != null)
        {
            Object.DestroyImmediate(existingStorePanel.gameObject);
        }
        CreateStorePanel(root, false);

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("[SceneElementSetup] UI-Canvas.prefab configurado en estilo pixel art nativo.");
    }

    private static void CreateGameOverPanel(GameObject canvasRoot)
    {
        var (panelObj, container, cg, rect) = CreatePixelPanelRoot(canvasRoot, "GameOverPanel", new Vector2(740f, 580f));

        var goc = panelObj.AddComponent<GameOverController>();
        SerializedObject soGOC = new SerializedObject(goc);

        soGOC.FindProperty("panelGroup").objectReferenceValue = cg;
        soGOC.FindProperty("panelRect").objectReferenceValue = rect;

        // Título
        CreatePixelText(container, "Title", "GAME OVER", 40f, new Vector2(0f, 215f), new Vector2(600f, 50f));

        // Botones de acción
        Button btnAd = CreatePixelButton(container, "WatchAdButton", "Watch Video (+2 Lives)", new Vector2(0f, 130f), new Vector2(440f, 68f));
        soGOC.FindProperty("watchAdButton").objectReferenceValue = btnAd;
        soGOC.FindProperty("firstSelectedButton").objectReferenceValue = btnAd.gameObject;

        Button btnTrebol = CreatePixelButton(container, "UseTrebolButton", "Spend Clover (Roll Dice)", new Vector2(0f, 50f), new Vector2(440f, 68f));
        soGOC.FindProperty("useTrebolButton").objectReferenceValue = btnTrebol;

        Button btnGems = CreatePixelButton(container, "SpendGemsButton", "Use Gems (+1 Life)", new Vector2(0f, -30f), new Vector2(440f, 68f));
        soGOC.FindProperty("spendGemsButton").objectReferenceValue = btnGems;

        Button btnMap = CreatePixelButton(container, "ReturnToMapButton", "Return to Map", new Vector2(0f, -110f), new Vector2(440f, 68f), isWarning: true);
        soGOC.FindProperty("returnToMapButton").objectReferenceValue = btnMap;

        // Fila de información económica
        GameObject txtCost = CreatePixelText(container, "GemsCostText", "10 Gems", 20f, new Vector2(170f, -30f), new Vector2(140f, 35f), new Color(1f, 0.88f, 0.2f));
        soGOC.FindProperty("gemCostText").objectReferenceValue = txtCost.GetComponent<TextMeshProUGUI>();

        GameObject txtCurrentGems = CreatePixelText(container, "CurrentGemsText", "Gems: 0", 22f, new Vector2(-130f, -190f), new Vector2(250f, 35f), new Color(1f, 0.9f, 0.3f));
        soGOC.FindProperty("currentGemsText").objectReferenceValue = txtCurrentGems.GetComponent<TextMeshProUGUI>();

        GameObject txtTrebol = CreatePixelText(container, "TrebolAvailableText", "Clovers: 0", 22f, new Vector2(130f, -190f), new Vector2(250f, 35f), new Color(0.3f, 1f, 0.4f));
        soGOC.FindProperty("trebolAvailableText").objectReferenceValue = txtTrebol.GetComponent<TextMeshProUGUI>();

        GameObject txtFeedback = CreatePixelText(container, "AdFeedbackText", "", 18f, new Vector2(0f, -235f), new Vector2(600f, 30f), new Color(1f, 0.35f, 0.35f));
        soGOC.FindProperty("adFeedbackText").objectReferenceValue = txtFeedback.GetComponent<TextMeshProUGUI>();

        soGOC.ApplyModifiedProperties();
        panelObj.SetActive(false);
    }

    private static void CreateDiceRollPanel(GameObject canvasRoot)
    {
        var (panelObj, container, cg, rect) = CreatePixelPanelRoot(canvasRoot, "DiceRollPanel", new Vector2(740f, 600f));

        var drc = panelObj.AddComponent<DiceRollController>();
        SerializedObject soDRC = new SerializedObject(drc);

        soDRC.FindProperty("panelGroup").objectReferenceValue = cg;
        soDRC.FindProperty("panelRect").objectReferenceValue = rect;

        // Título y Estado
        CreatePixelText(container, "DiceTitle", "DICE OF LIVES", 38f, new Vector2(0f, 235f), new Vector2(600f, 45f));
        GameObject statusText = CreatePixelText(container, "DiceStatus", "Normal Dice (1 - 3 Lives)", 22f, new Vector2(0f, 190f), new Vector2(600f, 35f));
        soDRC.FindProperty("diceStatusText").objectReferenceValue = statusText.GetComponent<TextMeshProUGUI>();

        // Marco del dado central
        GameObject diceBox = new GameObject("DiceBox");
        diceBox.transform.SetParent(container.transform, false);
        var dbRect = diceBox.AddComponent<RectTransform>();
        dbRect.anchorMin = new Vector2(0.5f, 0.5f);
        dbRect.anchorMax = new Vector2(0.5f, 0.5f);
        dbRect.pivot = new Vector2(0.5f, 0.5f);
        dbRect.sizeDelta = new Vector2(130f, 130f);
        dbRect.anchoredPosition = new Vector2(0f, 85f);

        var (normalBtn, _, _, _) = GetButtonSprites();
        var dbImg = diceBox.AddComponent<Image>();
        dbImg.sprite = normalBtn;
        dbImg.type = Image.Type.Sliced;
        dbImg.color = new Color(0.9f, 0.95f, 1f, 1f);

        // Imagen gráfica de la cara del dado
        GameObject diceFaceObj = new GameObject("DiceFaceImage");
        diceFaceObj.transform.SetParent(diceBox.transform, false);
        var dfRect = diceFaceObj.AddComponent<RectTransform>();
        dfRect.anchorMin = new Vector2(0.5f, 0.5f);
        dfRect.anchorMax = new Vector2(0.5f, 0.5f);
        dfRect.pivot = new Vector2(0.5f, 0.5f);
        dfRect.sizeDelta = new Vector2(100f, 100f);
        dfRect.anchoredPosition = Vector2.zero;

        Sprite[] diceSprites = GetDiceFaceSprites();
        var dfImg = diceFaceObj.AddComponent<Image>();
        if (diceSprites != null && diceSprites.Length > 0 && diceSprites[0] != null)
            dfImg.sprite = diceSprites[0];
        dfImg.preserveAspect = true;
        soDRC.FindProperty("diceFaceImage").objectReferenceValue = dfImg;

        SerializedProperty spritesProp = soDRC.FindProperty("diceFaceSprites");
        if (spritesProp != null && diceSprites != null)
        {
            spritesProp.arraySize = diceSprites.Length;
            for (int i = 0; i < diceSprites.Length; i++)
            {
                spritesProp.GetArrayElementAtIndex(i).objectReferenceValue = diceSprites[i];
            }
        }

        // Fallback número del dado
        GameObject numberText = CreatePixelText(diceBox, "DiceNumber", "?", 60f, Vector2.zero, new Vector2(100f, 100f));
        numberText.SetActive(false);
        soDRC.FindProperty("diceNumberText").objectReferenceValue = numberText.GetComponent<TextMeshProUGUI>();

        // Botones
        Button btnRoll = CreatePixelButton(container, "RollButton", "Roll Dice", new Vector2(-120f, -25f), new Vector2(210f, 66f));
        soDRC.FindProperty("rollButton").objectReferenceValue = btnRoll;

        Button btnStop = CreatePixelButton(container, "StopButton", "Stop", new Vector2(120f, -25f), new Vector2(210f, 66f), isWarning: true);
        btnStop.interactable = false;
        soDRC.FindProperty("stopButton").objectReferenceValue = btnStop;

        Button btnUpgrade = CreatePixelButton(container, "UpgradeButton", "Upgrade with Clover", new Vector2(0f, -105f), new Vector2(440f, 66f));
        soDRC.FindProperty("upgradeWithTrebolButton").objectReferenceValue = btnUpgrade;

        Button btnClose = CreatePixelButton(container, "CloseButton", "Continue", new Vector2(0f, -185f), new Vector2(440f, 66f), isWarning: true);
        btnClose.interactable = false;
        soDRC.FindProperty("closeButton").objectReferenceValue = btnClose;

        GameObject trebolText = CreatePixelText(container, "TrebolAvailableText", "Clovers: 0", 20f, new Vector2(0f, -250f), new Vector2(500f, 35f), new Color(0.3f, 1f, 0.4f));
        soDRC.FindProperty("trebolAvailableText").objectReferenceValue = trebolText.GetComponent<TextMeshProUGUI>();

        soDRC.ApplyModifiedProperties();
        panelObj.SetActive(false);
    }

    private static void CreateStorePanel(GameObject canvasRoot, bool createOpenButton = false)
    {
        var (panelObj, container, cg, rect) = CreatePixelPanelRoot(canvasRoot, "StorePanel", new Vector2(740f, 540f));

        var noAds = panelObj.AddComponent<NoAdsPurchaseButton>();
        SerializedObject soNoAds = new SerializedObject(noAds);
        soNoAds.FindProperty("storePanelGroup").objectReferenceValue = cg;

        // Título
        CreatePixelText(container, "StoreTitle", "STORE / REMOVE ADS", 36f, new Vector2(0f, 195f), new Vector2(600f, 45f));

        // Descripción
        CreatePixelText(container, "StoreDesc", "Play without interruptions!\nPurchasing permanently removes all ads.", 20f, new Vector2(0f, 125f), new Vector2(640f, 60f), null, allowWrapping: true);

        // Botón Compra
        Button btnPurchase = CreatePixelButton(container, "PurchaseButton", "Remove Ads", new Vector2(0f, 35f), new Vector2(440f, 72f));
        soNoAds.FindProperty("purchaseButton").objectReferenceValue = btnPurchase;
        TextMeshProUGUI purchaseLabel = btnPurchase.GetComponentInChildren<TextMeshProUGUI>();
        soNoAds.FindProperty("purchaseButtonLabel").objectReferenceValue = purchaseLabel;

        // Botón Restaurar
        Button btnRestore = CreatePixelButton(container, "RestoreButton", "Restore Purchases", new Vector2(0f, -50f), new Vector2(440f, 72f));
        soNoAds.FindProperty("restoreButton").objectReferenceValue = btnRestore;

        // Indicador activo (cuando ya está comprado)
        GameObject indicatorObj = CreatePixelText(container, "ActiveIndicator", "Remove Ads Active", 24f, new Vector2(0f, 35f), new Vector2(440f, 50f), new Color(0.3f, 1f, 0.4f));
        indicatorObj.SetActive(false);
        soNoAds.FindProperty("noAdsActiveIndicator").objectReferenceValue = indicatorObj;

        // Status text
        GameObject statusObj = CreatePixelText(container, "StatusText", "", 18f, new Vector2(0f, -125f), new Vector2(640f, 35f), new Color(1f, 0.9f, 0.3f));
        soNoAds.FindProperty("statusText").objectReferenceValue = statusObj.GetComponent<TextMeshProUGUI>();

        // Botón Cerrar
        Button btnClose = CreatePixelButton(container, "CloseButton", "Close", new Vector2(0f, -190f), new Vector2(440f, 68f), isWarning: true);
        soNoAds.FindProperty("closeStoreButton").objectReferenceValue = btnClose;

        soNoAds.ApplyModifiedProperties();
        panelObj.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  5. Gameplay Levels (Agregar TrebolPickup a cada nivel)
    // ─────────────────────────────────────────────────────────────────────────

    private static void SetupGameplayLevels(GameObject trebolPrefab)
    {
        for (int i = 0; i <= 10; i++)
        {
            string scenePath = $"Assets/Scenes/Levels/Level_{i}.unity";
            if (!File.Exists(scenePath)) continue;

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneDirty = false;

            if (trebolPrefab != null && Object.FindObjectOfType<TrebolPickup>() == null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(trebolPrefab);
                
                Goal goal = Object.FindObjectOfType<Goal>();
                if (goal != null)
                {
                    instance.transform.position = goal.transform.position + new Vector3(-3f, 2f, 0f);
                }
                else
                {
                    instance.transform.position = new Vector3(5f, 2f, 0f);
                }

                sceneDirty = true;
                Debug.Log($"[SceneElementSetup] TrebolPickup agregado a {scenePath}");
            }

            Camera cam = Object.FindObjectOfType<Camera>();
            Color skyColor = new Color(0.2784f, 0.9529f, 1f, 1f);
            if (cam != null && cam.backgroundColor != skyColor)
            {
                cam.backgroundColor = skyColor;
                sceneDirty = true;
            }

            if (sceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  6. Revisión exhaustiva de auto-sizing en todos los textos del proyecto
    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("CustomEditorTools/Configure All Texts For Auto Sizing & Resize")]
    public static void ConfigureAllProjectTextsForAutoSizing()
    {
        Debug.Log("[SceneElementSetup] Iniciando configuracion de auto-sizing (shrink/resize) en todos los textos...");

        string[] prefabsToScan = new string[]
        {
            "Assets/Prefabs/ReUsable/UI-Canvas.prefab",
            "Assets/Prefabs/ReUsable/LevelPrefab.prefab",
            "Assets/Prefabs/UI/Start-Button.prefab",
            "Assets/Prefabs/UI/PauseMenu.prefab"
        };

        foreach (string prefabPath in prefabsToScan)
        {
            if (!File.Exists(prefabPath)) continue;
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null) continue;

            bool modified = ApplyAutoSizingToHierarchy(root);
            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[SceneElementSetup] Auto-sizing y resize aplicados a prefab: {prefabPath}");
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        List<string> scenesToScan = new List<string>()
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/LevelSelection.unity",
            "Assets/Scenes/LevelSelection-Complete.unity"
        };

        for (int i = 0; i <= 10; i++)
        {
            string lvl = $"Assets/Scenes/Levels/Level_{i}.unity";
            if (File.Exists(lvl))
                scenesToScan.Add(lvl);
        }

        foreach (string scenePath in scenesToScan)
        {
            if (!File.Exists(scenePath)) continue;
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool modified = false;

            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if (ApplyAutoSizingToHierarchy(root))
                    modified = true;
            }

            if (modified)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[SceneElementSetup] Auto-sizing y resize aplicados a escena: {scenePath}");
            }
        }
    }

    private static bool ApplyAutoSizingToHierarchy(GameObject root)
    {
        bool modified = false;
        TextMeshProUGUI[] texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in texts)
        {
            string textContent = tmp.text ?? string.Empty;
            string objName = tmp.gameObject.name;

            bool isMultiline = textContent.Contains("\n") || objName.Contains("Desc") || objName.Contains("Billboard");
            float currentMax = Mathf.Max(tmp.fontSize, tmp.fontSizeMax, 18f);
            float minSize = currentMax > 60f ? 24f : 14f;

            if (!tmp.enableAutoSizing || tmp.fontSizeMin != minSize || tmp.fontSizeMax != currentMax)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = minSize;
                tmp.fontSizeMax = currentMax;
                modified = true;
            }

            TextWrappingModes targetWrap = isMultiline ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            if (tmp.textWrappingMode != targetWrap)
            {
                tmp.textWrappingMode = targetWrap;
                modified = true;
            }

            if (tmp.overflowMode != TextOverflowModes.Ellipsis)
            {
                tmp.overflowMode = TextOverflowModes.Ellipsis;
                modified = true;
            }

            // Si es un contador de gemas que quedó en 999 en diseño, resetear a 0
            if ((objName == "Gems_Txt" || objName.Contains("Gems")) && textContent == "999")
            {
                tmp.text = "0";
                modified = true;
            }
        }
        return modified;
    }
}
