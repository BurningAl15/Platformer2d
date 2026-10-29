using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Script de Editor para configurar automáticamente todos los elementos faltantes,
/// prefabs y placeholders en las escenas de Fox's Tale.
/// 
/// SE EJECUTA DESDE: Menú -> CustomEditorTools -> Setup Missing Scene Elements & Placeholders
/// o automáticamente al compilar el script una vez.
/// </summary>
[InitializeOnLoad]
public static class SceneElementSetup
{
    private const string SETUP_FLAG_KEY = "FoxsTale_SceneSetup_Completed_v1";

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
        Debug.Log("[SceneElementSetup] 🚀 Iniciando configuración de escenas y placeholders...");

        try
        {
            AssetDatabase.StartAssetEditing();

            // 1. Crear prefabs de coleccionables
            GameObject trebolPrefab = CreateTrebolPrefab();
            GameObject diceFragPrefab = CreateDiceFragmentPrefab();

            // 2. Configurar MainMenu
            SetupMainMenuScene();

            // 3. Configurar LevelSelection
            SetupLevelSelectionScene();

            // 4. Configurar UI-Canvas Prefab
            SetupUICanvasPrefab();

            // 5. Agregar Trebol a cada nivel de gameplay
            SetupGameplayLevels(trebolPrefab);

            Debug.Log("[SceneElementSetup] ✅ ¡Todas las escenas y placeholders configurados con éxito!");
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
    //  1. Prefabs de coleccionables
    // ─────────────────────────────────────────────────────────────────────────

    private static GameObject CreateTrebolPrefab()
    {
        string path = "Assets/Prefabs/TrebolPickup.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject go = new GameObject("TrebolPickup");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.2f, 0.85f, 0.3f, 1f); // Verde trébol
        sr.sortingLayerName = "Interactables";

        // Usar sprite de gema como base temporal si existe
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
        sr.color = new Color(1f, 0.84f, 0f, 1f); // Dorado dado
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
            bootObj.AddComponent<AppBootstrap>(); // [RequireComponent] agrega SaveManager, GamepadDetector, LivesManager
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

        // StorePanel & StoreButton en MainMenu Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null && canvas.transform.Find("StorePanel") == null)
        {
            CreateStorePanel(canvas.gameObject, true);
            modified = true;
            Debug.Log("[SceneElementSetup] StorePanel y Botón Quitar Anuncios creados en MainMenu.");
        }

        if (modified)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
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
                // Buscar si ya tiene un hijo Badge_Trebol
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

                    // Usar sprite de gem badge como base
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

        if (modified)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[SceneElementSetup] Trebol badges configurados en {mapPoints.Length} MapPoints de LevelSelection.");
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

        // 1. Configurar HUD Texts (Trebol y Vidas)
        Transform hudContainer = root.transform.Find("Gems_Txt")?.parent ?? root.transform;

        // Trebol HUD Text
        Transform trebolTxtTransform = root.transform.Find("Trebol_Txt");
        if (trebolTxtTransform == null)
        {
            GameObject trebolTxtObj = new GameObject("Trebol_Txt");
            trebolTxtObj.transform.SetParent(hudContainer, false);
            var rect = trebolTxtObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.85f, 0.9f);
            rect.anchorMax = new Vector2(0.98f, 0.98f);
            rect.anchoredPosition = Vector2.zero;

            var tmp = trebolTxtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "🍀 x0";
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Right;

            SerializedProperty trebolTextProp = soUI.FindProperty("trebolCountText");
            if (trebolTextProp != null) trebolTextProp.objectReferenceValue = tmp;
        }

        // Vidas HUD Text
        Transform livesTxtTransform = root.transform.Find("Lives_Txt");
        if (livesTxtTransform == null)
        {
            GameObject livesTxtObj = new GameObject("Lives_Txt");
            livesTxtObj.transform.SetParent(hudContainer, false);
            var rect = livesTxtObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.85f);
            rect.anchorMax = new Vector2(0.15f, 0.92f);
            rect.anchoredPosition = Vector2.zero;

            var tmp = livesTxtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "❤️ x3";
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Left;

            SerializedProperty livesTextProp = soUI.FindProperty("livesText");
            if (livesTextProp != null) livesTextProp.objectReferenceValue = tmp;
        }

        soUI.ApplyModifiedProperties();

        // 2. GameOverPanel
        Transform existingGOPanel = root.transform.Find("GameOverPanel");
        if (existingGOPanel == null)
        {
            CreateGameOverPanel(root);
        }

        // 3. DiceRollPanel
        Transform existingDicePanel = root.transform.Find("DiceRollPanel");
        if (existingDicePanel == null)
        {
            CreateDiceRollPanel(root);
        }

        // 4. MobileControls (Controles táctiles móviles serializados)
        Transform existingMobileControls = root.transform.Find("MobileControls");
        if (existingMobileControls == null)
        {
            CreateMobileControls(root);
        }

        // 5. StorePanel (Panel de Quitar Anuncios)
        Transform existingStorePanel = root.transform.Find("StorePanel");
        if (existingStorePanel == null)
        {
            CreateStorePanel(root, false);
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("[SceneElementSetup] UI-Canvas.prefab configurado con HUD, GameOverPanel, DiceRollPanel, MobileControls y StorePanel.");
    }

    private static void CreateGameOverPanel(GameObject canvasRoot)
    {
        GameObject panelObj = new GameObject("GameOverPanel");
        panelObj.transform.SetParent(canvasRoot.transform, false);

        var rect = panelObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var cg = panelObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        var img = panelObj.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.88f); // Fondo oscuro

        var goc = panelObj.AddComponent<GameOverController>();
        SerializedObject soGOC = new SerializedObject(goc);

        soGOC.FindProperty("panelGroup").objectReferenceValue = cg;
        soGOC.FindProperty("panelRect").objectReferenceValue = rect;

        // Título
        GameObject titleObj = CreateTMPText(panelObj, "Title", "¡GAME OVER!", 44, new Vector2(0f, 180f));

        // Botón Ad
        Button btnAd = CreateUIButton(panelObj, "WatchAdButton", "Ver Video (+2 Vidas)", new Vector2(0f, 80f), new Color(0.2f, 0.6f, 0.9f));
        soGOC.FindProperty("watchAdButton").objectReferenceValue = btnAd;
        soGOC.FindProperty("firstSelectedButton").objectReferenceValue = btnAd.gameObject;

        // Botón Trébol
        Button btnTrebol = CreateUIButton(panelObj, "UseTrebolButton", "🍀 Gastar Trébol (Tirar Dado)", new Vector2(0f, 10f), new Color(0.2f, 0.8f, 0.3f));
        soGOC.FindProperty("useTrebolButton").objectReferenceValue = btnTrebol;

        // Botón Gemas
        Button btnGems = CreateUIButton(panelObj, "SpendGemsButton", "💎 Usar Gemas (+1 Vida)", new Vector2(0f, -60f), new Color(0.8f, 0.4f, 0.8f));
        soGOC.FindProperty("spendGemsButton").objectReferenceValue = btnGems;

        // Botón Volver al Mapa
        Button btnMap = CreateUIButton(panelObj, "ReturnToMapButton", "Volver al Mapa", new Vector2(0f, -130f), new Color(0.6f, 0.6f, 0.6f));
        soGOC.FindProperty("returnToMapButton").objectReferenceValue = btnMap;

        // Textos informativos
        GameObject txtGems = CreateTMPText(panelObj, "GemsCostText", "10 💎", 22, new Vector2(160f, -60f));
        soGOC.FindProperty("gemCostText").objectReferenceValue = txtGems.GetComponent<TextMeshProUGUI>();

        GameObject txtCurrentGems = CreateTMPText(panelObj, "CurrentGemsText", "Tienes: 0 💎", 22, new Vector2(0f, -200f));
        soGOC.FindProperty("currentGemsText").objectReferenceValue = txtCurrentGems.GetComponent<TextMeshProUGUI>();

        GameObject txtTrebol = CreateTMPText(panelObj, "TrebolAvailableText", "Tréboles: 0 🍀", 22, new Vector2(0f, -230f));
        soGOC.FindProperty("trebolAvailableText").objectReferenceValue = txtTrebol.GetComponent<TextMeshProUGUI>();

        soGOC.ApplyModifiedProperties();
    }

    private static void CreateDiceRollPanel(GameObject canvasRoot)
    {
        GameObject panelObj = new GameObject("DiceRollPanel");
        panelObj.transform.SetParent(canvasRoot.transform, false);

        var rect = panelObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var cg = panelObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        var img = panelObj.AddComponent<Image>();
        img.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);

        var drc = panelObj.AddComponent<DiceRollController>();
        SerializedObject soDRC = new SerializedObject(drc);

        soDRC.FindProperty("panelGroup").objectReferenceValue = cg;
        soDRC.FindProperty("panelRect").objectReferenceValue = rect;

        // Título y Estado
        CreateTMPText(panelObj, "DiceTitle", "🎲 DADO DE VIDAS 🎲", 40, new Vector2(0f, 180f));
        GameObject statusText = CreateTMPText(panelObj, "DiceStatus", "Dado Normal (1 - 3 Vidas)", 26, new Vector2(0f, 120f));
        soDRC.FindProperty("diceStatusText").objectReferenceValue = statusText.GetComponent<TextMeshProUGUI>();

        // Número del dado (grande)
        GameObject numberText = CreateTMPText(panelObj, "DiceNumber", "?", 80, new Vector2(0f, 20f));
        soDRC.FindProperty("diceNumberText").objectReferenceValue = numberText.GetComponent<TextMeshProUGUI>();

        // Botones
        Button btnRoll = CreateUIButton(panelObj, "RollButton", "¡Lanzar Dado!", new Vector2(-120f, -90f), new Color(0.2f, 0.8f, 0.3f));
        soDRC.FindProperty("rollButton").objectReferenceValue = btnRoll;

        Button btnStop = CreateUIButton(panelObj, "StopButton", "Parar", new Vector2(120f, -90f), new Color(0.9f, 0.3f, 0.2f));
        btnStop.interactable = false;
        soDRC.FindProperty("stopButton").objectReferenceValue = btnStop;

        Button btnUpgrade = CreateUIButton(panelObj, "UpgradeButton", "🍀 Mejorar con Trébol", new Vector2(0f, -160f), new Color(0.85f, 0.7f, 0.1f));
        soDRC.FindProperty("upgradeWithTrebolButton").objectReferenceValue = btnUpgrade;

        Button btnClose = CreateUIButton(panelObj, "CloseButton", "Continuar", new Vector2(0f, -220f), new Color(0.5f, 0.5f, 0.5f));
        btnClose.interactable = false;
        soDRC.FindProperty("closeButton").objectReferenceValue = btnClose;

        GameObject trebolText = CreateTMPText(panelObj, "TrebolAvailableText", "🍀 Tréboles: 0", 22, new Vector2(0f, -260f));
        soDRC.FindProperty("trebolAvailableText").objectReferenceValue = trebolText.GetComponent<TextMeshProUGUI>();

        soDRC.ApplyModifiedProperties();
    }

    private static void CreateMobileControls(GameObject canvasRoot)
    {
        GameObject controlsObj = new GameObject("MobileControls");
        controlsObj.transform.SetParent(canvasRoot.transform, false);

        var rect = controlsObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var cg = controlsObj.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        var mgc = controlsObj.AddComponent<MobileGameplayControls>();

        // Joystick (Move Stick)
        GameObject stickObj = new GameObject("MoveStick");
        stickObj.transform.SetParent(controlsObj.transform, false);
        var stickRect = stickObj.AddComponent<RectTransform>();
        stickRect.anchorMin = Vector2.zero;
        stickRect.anchorMax = Vector2.zero;
        stickRect.pivot = new Vector2(0.5f, 0.5f);
        stickRect.anchoredPosition = new Vector2(160f, 150f);
        stickRect.sizeDelta = new Vector2(200f, 200f);

        var stickImg = stickObj.AddComponent<Image>();
        stickImg.color = new Color(0.06f, 0.13f, 0.17f, 0.48f);

        var stick = stickObj.AddComponent<OnScreenStick>();
        stick.controlPath = "<Gamepad>/leftStick";
        stick.behaviour = OnScreenStick.Behaviour.RelativePositionWithStaticOrigin;
        stick.movementRange = 60f;
        stick.useIsolatedInputActions = true;

        // Knob
        GameObject knobObj = new GameObject("StickKnob");
        knobObj.transform.SetParent(stickObj.transform, false);
        var knobRect = knobObj.AddComponent<RectTransform>();
        knobRect.anchorMin = new Vector2(0.5f, 0.5f);
        knobRect.anchorMax = new Vector2(0.5f, 0.5f);
        knobRect.pivot = new Vector2(0.5f, 0.5f);
        knobRect.anchoredPosition = Vector2.zero;
        knobRect.sizeDelta = new Vector2(80f, 80f);

        var knobImg = knobObj.AddComponent<Image>();
        knobImg.color = new Color(0.85f, 0.94f, 0.92f, 0.62f);
        knobImg.raycastTarget = false;

        // Jump Button
        GameObject jumpObj = new GameObject("JumpButton");
        jumpObj.transform.SetParent(controlsObj.transform, false);
        var jumpRect = jumpObj.AddComponent<RectTransform>();
        jumpRect.anchorMin = new Vector2(1f, 0f);
        jumpRect.anchorMax = new Vector2(1f, 0f);
        jumpRect.pivot = new Vector2(0.5f, 0.5f);
        jumpRect.anchoredPosition = new Vector2(-160f, 150f);
        jumpRect.sizeDelta = new Vector2(150f, 150f);

        var jumpImg = jumpObj.AddComponent<Image>();
        jumpImg.color = new Color(0.88f, 0.25f, 0.17f, 0.72f);

        var jumpBtn = jumpObj.AddComponent<OnScreenButton>();
        jumpBtn.controlPath = "<Gamepad>/buttonSouth";

        CreateTMPText(jumpObj, "Label", "A", 48, Vector2.zero);

        // Pause Button
        GameObject pauseObj = new GameObject("PauseButton");
        pauseObj.transform.SetParent(controlsObj.transform, false);
        var pauseRect = pauseObj.AddComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(0.5f, 1f);
        pauseRect.anchorMax = new Vector2(0.5f, 1f);
        pauseRect.pivot = new Vector2(0.5f, 0.5f);
        pauseRect.anchoredPosition = new Vector2(0f, -70f);
        pauseRect.sizeDelta = new Vector2(80f, 80f);

        var pauseImg = pauseObj.AddComponent<Image>();
        pauseImg.color = new Color(0.06f, 0.13f, 0.17f, 0.5f);

        var pauseBtn = pauseObj.AddComponent<OnScreenButton>();
        pauseBtn.controlPath = "<Gamepad>/start";

        CreateTMPText(pauseObj, "Label", "II", 32, Vector2.zero);

        SerializedObject soMGC = new SerializedObject(mgc);
        soMGC.FindProperty("controlsCanvasGroup").objectReferenceValue = cg;
        soMGC.FindProperty("onScreenStick").objectReferenceValue = stick;
        soMGC.FindProperty("jumpButton").objectReferenceValue = jumpBtn;
        soMGC.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
        soMGC.ApplyModifiedProperties();
    }

    private static void CreateStorePanel(GameObject canvasRoot, bool createOpenButton)
    {
        GameObject panelObj = new GameObject("StorePanel");
        panelObj.transform.SetParent(canvasRoot.transform, false);

        var rect = panelObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var cg = panelObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        var img = panelObj.AddComponent<Image>();
        img.color = new Color(0.04f, 0.04f, 0.08f, 0.95f);

        var noAds = panelObj.AddComponent<NoAdsPurchaseButton>();
        SerializedObject soNoAds = new SerializedObject(noAds);
        soNoAds.FindProperty("storePanelGroup").objectReferenceValue = cg;

        // Título
        CreateTMPText(panelObj, "StoreTitle", "TIENDA / QUITAR ANUNCIOS", 38, new Vector2(0f, 180f));

        // Descripción
        CreateTMPText(panelObj, "StoreDesc", "¡Juega sin interrupciones!\nElimina todos los anuncios permanentemente.", 22, new Vector2(0f, 100f));

        // Botón Compra
        Button btnPurchase = CreateUIButton(panelObj, "PurchaseButton", "Sin Anuncios", new Vector2(0f, 10f), new Color(0.2f, 0.75f, 0.35f));
        soNoAds.FindProperty("purchaseButton").objectReferenceValue = btnPurchase;
        TextMeshProUGUI purchaseLabel = btnPurchase.GetComponentInChildren<TextMeshProUGUI>();
        soNoAds.FindProperty("purchaseButtonLabel").objectReferenceValue = purchaseLabel;

        // Botón Restaurar
        Button btnRestore = CreateUIButton(panelObj, "RestoreButton", "Restaurar Compras", new Vector2(0f, -60f), new Color(0.3f, 0.5f, 0.8f));
        soNoAds.FindProperty("restoreButton").objectReferenceValue = btnRestore;

        // Indicador activo (cuando ya está comprado)
        GameObject indicatorObj = CreateTMPText(panelObj, "ActiveIndicator", "✓ Sin Anuncios Activo", 24, new Vector2(0f, 10f));
        indicatorObj.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 1f, 0.4f);
        indicatorObj.SetActive(false);
        soNoAds.FindProperty("noAdsActiveIndicator").objectReferenceValue = indicatorObj;

        // Status text
        GameObject statusObj = CreateTMPText(panelObj, "StatusText", "", 18, new Vector2(0f, -120f));
        soNoAds.FindProperty("statusText").objectReferenceValue = statusObj.GetComponent<TextMeshProUGUI>();

        // Botón Cerrar
        Button btnClose = CreateUIButton(panelObj, "CloseButton", "Cerrar", new Vector2(0f, -180f), new Color(0.5f, 0.5f, 0.5f));
        soNoAds.FindProperty("closeStoreButton").objectReferenceValue = btnClose;

        // Botón para abrir la tienda (si se solicita, ej. en MainMenu)
        if (createOpenButton)
        {
            Button btnOpen = CreateUIButton(canvasRoot, "OpenStoreButton", "🛒 Quitar Anuncios", new Vector2(0f, -180f), new Color(0.9f, 0.6f, 0.1f));
            soNoAds.FindProperty("openStoreButton").objectReferenceValue = btnOpen;
        }

        soNoAds.ApplyModifiedProperties();
    }

    private static GameObject CreateTMPText(GameObject parent, string name, string text, float fontSize, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500f, 60f);
        rect.anchoredPosition = anchoredPos;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return go;
    }

    private static Button CreateUIButton(GameObject parent, string name, string label, Vector2 anchoredPos, Color btnColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(260f, 50f);
        rect.anchoredPosition = anchoredPos;

        var img = go.AddComponent<Image>();
        img.color = btnColor;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(go.transform, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btn;
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
                
                // Ubicar el trébol en una posición de exploración (cerca del Goal o a altura media)
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

            // Fondo y encuadre: fijar color de cielo #47F3FF en la cámara para eliminar cualquier corte cian/azul
            Camera cam = Object.FindObjectOfType<Camera>();
            Color skyColor = new Color(0.2784f, 0.9529f, 1f, 1f);
            if (cam != null && cam.backgroundColor != skyColor)
            {
                cam.backgroundColor = skyColor;
                sceneDirty = true;
                Debug.Log($"[SceneElementSetup] Cámara de {scenePath} configurada con color de cielo #47F3FF.");
            }

            if (sceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
    }
}
