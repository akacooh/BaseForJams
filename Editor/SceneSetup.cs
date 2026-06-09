using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace BaseForJams.Editor
{
    /// <summary>
    /// One-click scene setup for the Base For Jams package. Idempotent — re-running
    /// rebuilds the StartScreen and PauseMenu roots.
    /// Menu: Tools ▶ Base For Jams ▶ Setup Scene
    /// </summary>
    public static class SceneSetup
    {
        private const string MenuPath = "Tools/Base For Jams/Setup Scene";

        [MenuItem(MenuPath)]
        public static void SetupInScene()
        {
            // ── GameStateManager (+ input) ────────────────────────────────────
            GameStateManager gameState = Object.FindAnyObjectByType<GameStateManager>();
            if (gameState == null)
            {
                var gsGO = new GameObject("GameStateManager");
                gameState = gsGO.AddComponent<GameStateManager>();
                Undo.RegisterCreatedObjectUndo(gsGO, "Create GameStateManager");
            }
            if (gameState.GetComponent<PauseInput>() == null)
                Undo.AddComponent<PauseInput>(gameState.gameObject);

            // ── EventSystem ───────────────────────────────────────────────────
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                esGO.AddComponent<InputSystemUIInputModule>();
#else
                esGO.AddComponent<StandaloneInputModule>();
#endif
                Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
            }

            // ── Canvas ────────────────────────────────────────────────────────
            Canvas canvas = null;
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay) { canvas = c; break; }
            }
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGO.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
            }

            // ── Idempotent: remove previously generated roots ─────────────────
            DestroyChildIfExists(canvas.transform, "StartScreen");
            DestroyChildIfExists(canvas.transform, "PauseMenu");

            // Locate (or create) a SettingsData asset to auto-assign
            SettingsData settings = FindOrCreateSettingsAsset();

            // StartScreen first (renders behind), PauseMenu second (on top → pause
            // button stays clickable while the start screen is visible).
            BuildStartScreen(canvas.transform);
            BuildPauseMenu(canvas.transform, gameState, settings);

            Selection.activeObject = canvas.gameObject;
            Debug.Log("[BaseForJams] Scene setup complete (StartScreen + PauseMenu + Restart + Quit).");
        }

        // ── Builders ──────────────────────────────────────────────────────────
        private static void BuildStartScreen(Transform canvas)
        {
            var rootGO = new GameObject("StartScreen");
            Undo.RegisterCreatedObjectUndo(rootGO, "Create StartScreen");
            rootGO.transform.SetParent(canvas, false);
            StretchFull(rootGO.AddComponent<RectTransform>());
            var screen = rootGO.AddComponent<StartScreen>();

            // Opaque background (covers the game until Play is pressed)
            rootGO.AddComponent<Image>().color = new Color(0.07f, 0.08f, 0.10f, 1f);

            // Title
            var titleGO = CreateText(rootGO.transform, "Title", "GAME TITLE", 48, TextAnchor.MiddleCenter);
            var tr = titleGO.GetComponent<RectTransform>();
            tr.anchorMin = tr.anchorMax = tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(700, 90);
            tr.anchoredPosition = new Vector2(0, -130);

            // Play button
            var playGO = CreateButton(rootGO.transform, "PlayButton", "Play");
            var pr = playGO.GetComponent<RectTransform>();
            pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 0.5f);
            pr.sizeDelta = new Vector2(220, 60);
            pr.anchoredPosition = Vector2.zero;

            var so = new SerializedObject(screen);
            so.FindProperty("screenRoot").objectReferenceValue = rootGO;
            so.FindProperty("playButton").objectReferenceValue = playGO.GetComponent<Button>();
            so.FindProperty("titleText").objectReferenceValue  = titleGO.GetComponent<Text>();
            so.ApplyModifiedProperties();
        }

        private static void BuildPauseMenu(Transform canvas, GameStateManager gameState, SettingsData settings)
        {
            var rootGO = new GameObject("PauseMenu");
            Undo.RegisterCreatedObjectUndo(rootGO, "Create PauseMenu");
            rootGO.transform.SetParent(canvas, false);
            StretchFull(rootGO.AddComponent<RectTransform>());
            var menuUI = rootGO.AddComponent<PauseMenu>();

            // Pause button (top-right corner)
            var pauseBtnGO = CreateButton(rootGO.transform, "PauseButton", "II");
            var pbRect = pauseBtnGO.GetComponent<RectTransform>();
            pbRect.anchorMin = pbRect.anchorMax = pbRect.pivot = new Vector2(1, 1);
            pbRect.sizeDelta = new Vector2(60, 40);
            pbRect.anchoredPosition = new Vector2(-10, -10);

            // Panel
            var panelGO = new GameObject("PausePanel");
            panelGO.transform.SetParent(rootGO.transform, false);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(360, 300);
            panelGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.92f);

            // Header
            var headerGO = CreateText(panelGO.transform, "Header", "Paused", 28, TextAnchor.MiddleCenter);
            var hRect = headerGO.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0, 1); hRect.anchorMax = new Vector2(1, 1); hRect.pivot = new Vector2(0.5f, 1);
            hRect.sizeDelta = new Vector2(0, 40); hRect.anchoredPosition = new Vector2(0, -12);

            // Settings container (expandable — add more SettingPanel rows here)
            var containerGO = new GameObject("SettingsContainer");
            containerGO.transform.SetParent(panelGO.transform, false);
            var cRect = containerGO.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0, 1); cRect.anchorMax = new Vector2(1, 1); cRect.pivot = new Vector2(0.5f, 1);
            cRect.offsetMin = new Vector2(20, 0); cRect.offsetMax = new Vector2(-20, 0);
            cRect.sizeDelta = new Vector2(-40, 160); cRect.anchoredPosition = new Vector2(0, -64);
            var vlg = containerGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childControlWidth = true;  vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            // One row per setting — add more lines here as you create new panels.
            BuildSettingRow<VolumePanel>(containerGO.transform,      "Volume",      "volumeSlider");
            BuildSettingRow<SensitivityPanel>(containerGO.transform, "Sensitivity", "sensitivitySlider");

            // Button row: Resume | Restart | Quit
            var rowGO = new GameObject("ButtonRow");
            rowGO.transform.SetParent(panelGO.transform, false);
            var rowRect = rowGO.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 0); rowRect.anchorMax = new Vector2(1, 0); rowRect.pivot = new Vector2(0.5f, 0);
            rowRect.offsetMin = new Vector2(20, 0); rowRect.offsetMax = new Vector2(-20, 0);
            rowRect.sizeDelta = new Vector2(-40, 48); rowRect.anchoredPosition = new Vector2(0, 16);
            var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childControlWidth = true;  hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

            var resumeBtnGO  = CreateButton(rowGO.transform, "ResumeButton",  "Resume");
            var restartBtnGO = CreateButton(rowGO.transform, "RestartButton", "Restart");
            var quitBtnGO    = CreateButton(rowGO.transform, "QuitButton",    "Quit");
            quitBtnGO.GetComponent<Image>().color = new Color(0.5f, 0.2f, 0.2f, 1f); // red-ish

            panelGO.SetActive(false);

            // Wire serialized fields
            var so = new SerializedObject(menuUI);
            so.FindProperty("gameState").objectReferenceValue    = gameState;
            so.FindProperty("settingsData").objectReferenceValue = settings;
            so.FindProperty("pausePanel").objectReferenceValue   = panelGO;
            so.FindProperty("pauseButton").objectReferenceValue  = pauseBtnGO.GetComponent<Button>();
            so.FindProperty("resumeButton").objectReferenceValue  = resumeBtnGO.GetComponent<Button>();
            so.FindProperty("restartButton").objectReferenceValue = restartBtnGO.GetComponent<Button>();
            so.FindProperty("quitButton").objectReferenceValue    = quitBtnGO.GetComponent<Button>();
            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// Builds one labelled "Label + Slider" row, attaches the given SettingPanel
        /// component, and wires the slider into its serialized field.
        /// </summary>
        private static void BuildSettingRow<T>(Transform parent, string label, string sliderFieldName)
            where T : SettingPanel
        {
            var rowGO = new GameObject(typeof(T).Name);
            rowGO.transform.SetParent(parent, false);
            rowGO.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 40);
            var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = true;  hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;
            var rowElement = rowGO.AddComponent<LayoutElement>();
            rowElement.minHeight = 30; rowElement.preferredHeight = 36;

            var labelGO = CreateText(rowGO.transform, "Label", label, 18, TextAnchor.MiddleLeft);
            var labelElement = labelGO.AddComponent<LayoutElement>();
            labelElement.preferredWidth = 100; labelElement.flexibleWidth = 0;

            var sliderGO = CreateSlider(rowGO.transform, "Slider");
            var sliderElement = sliderGO.AddComponent<LayoutElement>();
            sliderElement.minWidth = 120; sliderElement.preferredWidth = 200; sliderElement.flexibleWidth = 1;

            var panel = rowGO.AddComponent<T>();
            var so = new SerializedObject(panel);
            so.FindProperty(sliderFieldName).objectReferenceValue = sliderGO.GetComponent<Slider>();
            so.ApplyModifiedProperties();
        }

        // ── Generic UI helpers ──────────────────────────────────────────────────
        private static GameObject CreateButton(Transform parent, string name, string label)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.25f, 1f);
            go.AddComponent<Button>();

            var textGO = CreateText(go.transform, "Text", label, 18, TextAnchor.MiddleCenter);
            StretchFull(textGO.GetComponent<RectTransform>());

            return go;
        }

        private static GameObject CreateText(Transform parent, string name, string content, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var text = go.AddComponent<Text>();
            text.text      = content;
            text.color     = Color.white;
            text.alignment = anchor;
            text.fontSize  = fontSize;
            text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow   = VerticalWrapMode.Overflow;
            return go;
        }

        private static GameObject CreateSlider(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 20);

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(go.transform, false);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f); bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
            bgGO.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

            var fillAreaGO = new GameObject("Fill Area");
            fillAreaGO.transform.SetParent(go.transform, false);
            var fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f); fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5, 0); fillAreaRect.offsetMax = new Vector2(-5, 0);

            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            var fillRect = fillGO.AddComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;
            fillGO.AddComponent<Image>().color = new Color(0.3f, 0.7f, 1f, 1f);

            var handleAreaGO = new GameObject("Handle Slide Area");
            handleAreaGO.transform.SetParent(go.transform, false);
            var handleAreaRect = handleAreaGO.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero; handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10, 0); handleAreaRect.offsetMax = new Vector2(-10, 0);

            var handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(handleAreaGO.transform, false);
            var handleRect = handleGO.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);
            var handleImg = handleGO.AddComponent<Image>();
            handleImg.color = Color.white;

            var slider = go.AddComponent<Slider>();
            slider.fillRect      = fillRect;
            slider.handleRect    = handleRect;
            slider.targetGraphic = handleImg;
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;
            slider.direction = Slider.Direction.LeftToRight;

            return go;
        }

        // ── Misc helpers ────────────────────────────────────────────────────────
        private static void StretchFull(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        private static void DestroyChildIfExists(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null)
                Undo.DestroyObjectImmediate(child.gameObject);
        }

        private static SettingsData FindOrCreateSettingsAsset()
        {
            // Only search Assets/ — an asset that lives inside an installed package
            // is immutable, so the user could never edit its defaults.
            var guids = AssetDatabase.FindAssets("t:SettingsData", new[] { "Assets" });
            if (guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<SettingsData>(AssetDatabase.GUIDToAssetPath(guids[0]));

            var settings = ScriptableObject.CreateInstance<SettingsData>();
            AssetDatabase.CreateAsset(settings, "Assets/SettingsData.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("[BaseForJams] Created settings asset at Assets/SettingsData.asset");
            return settings;
        }
    }
}
