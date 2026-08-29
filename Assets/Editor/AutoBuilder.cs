using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using LexiGrid.UI;
using LexiGrid.Core;
using UnityEditor.SceneManagement;

public class AutoBuilder
{
    // Color palette
    static readonly Color BgDark = new Color(0.12f, 0.14f, 0.18f);       // #1E2330
    static readonly Color PanelBg = new Color(0.15f, 0.17f, 0.22f);      // #262B38
    static readonly Color AccentBlue = new Color(0.29f, 0.62f, 1f);      // #4A9EFF
    static readonly Color AccentGreen = new Color(0.29f, 1f, 0.50f);     // #4AFF7E
    static readonly Color ButtonBg = new Color(0.22f, 0.25f, 0.32f);     // #383F51
    static readonly Color TextWhite = Color.white;
    static readonly Color TextGray = new Color(0.6f, 0.6f, 0.65f);
    static readonly Color TileColor = new Color(0.95f, 0.95f, 0.95f);

    [MenuItem("LexiGrid/Auto-Build Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ===== CAMERA =====
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgDark;
        cam.orthographic = true;
        camObj.tag = "MainCamera";

        // ===== CANVAS =====
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

        // ===== EVENT SYSTEM =====
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ===== TILE PREFAB =====
        GameObject tilePrefabObj = CreateTilePrefab();
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        GameObject savedTilePrefab = PrefabUtility.SaveAsPrefabAsset(tilePrefabObj, "Assets/Prefabs/Tile.prefab");
        GameObject.DestroyImmediate(tilePrefabObj);

        // ===== LEADERBOARD ENTRY PREFAB =====
        GameObject entryPrefabObj = CreateLeaderboardEntryPrefab();
        GameObject savedEntryPrefab = PrefabUtility.SaveAsPrefabAsset(entryPrefabObj, "Assets/Prefabs/LeaderboardEntry.prefab");
        GameObject.DestroyImmediate(entryPrefabObj);

        // ===== SCREEN MANAGER =====
        GameObject smObj = new GameObject("ScreenManager");
        smObj.transform.SetParent(canvasObj.transform, false);
        ScreenManager sm = smObj.AddComponent<ScreenManager>();
        SerializedObject smSO = new SerializedObject(sm);

        // ===== BUILD ALL SCREENS =====
        CanvasGroup splashCG = BuildSplashScreen(canvasObj.transform, out SplashScreen splashScript);
        CanvasGroup mainMenuCG = BuildMainMenuScreen(canvasObj.transform, out MainMenuScreen menuScript);
        CanvasGroup gameCG = BuildGameScreen(canvasObj.transform, savedTilePrefab, raycaster,
            out GridView gridView, out InputManager inputManager, out HUDView hudView);
        CanvasGroup gameOverCG = BuildGameOverScreen(canvasObj.transform, out GameOverScreen goScript);
        CanvasGroup leaderboardCG = BuildLeaderboardScreen(canvasObj.transform, savedEntryPrefab, out LeaderboardView lbView);
        CanvasGroup howToPlayCG = BuildHowToPlayScreen(canvasObj.transform, out HowToPlayScreen htpScript);
        CanvasGroup settingsCG = BuildSettingsScreen(canvasObj.transform, out SettingsScreen settingsScript);

        // Wire ScreenManager
        smSO.FindProperty("_splashScreen").objectReferenceValue = splashCG;
        smSO.FindProperty("_mainMenuScreen").objectReferenceValue = mainMenuCG;
        smSO.FindProperty("_gameScreen").objectReferenceValue = gameCG;
        smSO.FindProperty("_gameOverScreen").objectReferenceValue = gameOverCG;
        smSO.FindProperty("_leaderboardScreen").objectReferenceValue = leaderboardCG;
        smSO.FindProperty("_howToPlayScreen").objectReferenceValue = howToPlayCG;
        smSO.FindProperty("_settingsScreen").objectReferenceValue = settingsCG;
        smSO.ApplyModifiedProperties();

        // ===== GAME MANAGER =====
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        TextAsset wordlist = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/wordlist.txt");

        SerializedObject gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_screenManager").objectReferenceValue = sm;
        gmSO.FindProperty("_gridView").objectReferenceValue = gridView;
        gmSO.FindProperty("_inputManager").objectReferenceValue = inputManager;
        gmSO.FindProperty("_hudView").objectReferenceValue = hudView;
        gmSO.FindProperty("_leaderboardView").objectReferenceValue = lbView;
        gmSO.FindProperty("_splashScreen").objectReferenceValue = splashScript;
        gmSO.FindProperty("_gameOverScreen").objectReferenceValue = goScript;
        gmSO.FindProperty("_wordListAsset").objectReferenceValue = wordlist;
        gmSO.FindProperty("_gridSize").intValue = 4;
        gmSO.FindProperty("_roundDuration").floatValue = 60f;
        gmSO.FindProperty("_leaderboardMaxSize").intValue = 10;
        gmSO.ApplyModifiedProperties();

        // ===== SAVE =====
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
        Debug.Log("LexiGrid full app built! Press Play.");
    }

    // ================================================================
    //  SCREEN BUILDERS
    // ================================================================

    static CanvasGroup BuildSplashScreen(Transform parent, out SplashScreen script)
    {
        GameObject panel = CreateScreenPanel(parent, "SplashScreen");
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        // Title
        CreateText(panel.transform, "Title", "LEXIGRID", 100, TextWhite, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 200), new Vector2(800, 120));

        // Tagline
        CreateText(panel.transform, "Tagline", "Find the Words", 40, TextGray, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(600, 60));

        // Loading bar
        GameObject sliderObj = new GameObject("LoadingBar");
        sliderObj.transform.SetParent(panel.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0, -50);
        sliderRect.sizeDelta = new Vector2(600, 30);

        // Slider background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = ButtonBg;
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.sizeDelta = Vector2.zero;

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero; fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(5, 5); fillAreaRect.offsetMax = new Vector2(-5, -5);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = AccentBlue;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one; fillRect.sizeDelta = Vector2.zero;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.targetGraphic = bgImg;
        slider.minValue = 0; slider.maxValue = 1; slider.value = 0;
        slider.interactable = false;

        // Loading text
        TextMeshProUGUI loadingTxt = CreateText(panel.transform, "LoadingText", "Loading dictionary... 0%",
            32, TextGray, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(600, 50));

        // Wire script
        script = panel.AddComponent<SplashScreen>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("_loadingBar").objectReferenceValue = slider;
        so.FindProperty("_loadingText").objectReferenceValue = loadingTxt;
        so.ApplyModifiedProperties();

        return cg;
    }

    static CanvasGroup BuildMainMenuScreen(Transform parent, out MainMenuScreen script)
    {
        GameObject panel = CreateScreenPanel(parent, "MainMenuScreen");
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        // Title
        CreateText(panel.transform, "Title", "LEXIGRID", 90, AccentBlue, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(800, 120));

        // Subtitle
        CreateText(panel.transform, "Subtitle", "Word Puzzle", 36, TextGray, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -270), new Vector2(400, 50));

        // Buttons
        Button playBtn = CreateMenuButton(panel.transform, "PlayButton", "PLAY", AccentBlue, new Vector2(0, 80));
        Button lbBtn = CreateMenuButton(panel.transform, "LeaderboardButton", "LEADERBOARD", ButtonBg, new Vector2(0, -40));
        Button htpBtn = CreateMenuButton(panel.transform, "HowToPlayButton", "HOW TO PLAY", ButtonBg, new Vector2(0, -160));
        Button settingsBtn = CreateMenuButton(panel.transform, "SettingsButton", "SETTINGS", ButtonBg, new Vector2(0, -280));

        // Best score
        TextMeshProUGUI bestScoreTxt = CreateText(panel.transform, "BestScoreText", "",
            36, TextGray, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(400, 60));

        script = panel.AddComponent<MainMenuScreen>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("_playButton").objectReferenceValue = playBtn;
        so.FindProperty("_leaderboardButton").objectReferenceValue = lbBtn;
        so.FindProperty("_howToPlayButton").objectReferenceValue = htpBtn;
        so.FindProperty("_settingsButton").objectReferenceValue = settingsBtn;
        so.FindProperty("_bestScoreText").objectReferenceValue = bestScoreTxt;
        so.ApplyModifiedProperties();

        return cg;
    }

    static CanvasGroup BuildGameScreen(Transform parent, GameObject tilePrefab, GraphicRaycaster raycaster,
        out GridView gridView, out InputManager inputManager, out HUDView hudView)
    {
        GameObject panel = CreateScreenPanel(parent, "GameScreen");
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        // -- Top Bar --
        GameObject topBar = new GameObject("TopBar");
        topBar.transform.SetParent(panel.transform, false);
        Image topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = new Color(0.1f, 0.12f, 0.16f, 0.9f);
        topBarImg.raycastTarget = false;
        RectTransform topBarRect = topBar.GetComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0, 1); topBarRect.anchorMax = new Vector2(1, 1);
        topBarRect.pivot = new Vector2(0.5f, 1);
        topBarRect.anchoredPosition = Vector2.zero; topBarRect.sizeDelta = new Vector2(0, 140);

        // Menu button in top bar
        Button menuBtn = CreateSmallButton(topBar.transform, "MenuButton", "✕", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(80, 80));

        TextMeshProUGUI scoreTxt = CreateText(topBar.transform, "ScoreText", "Score: 0", 48, TextWhite,
            TextAlignmentOptions.MidlineLeft, new Vector2(0, 0), new Vector2(0.4f, 1),
            Vector2.zero, Vector2.zero, true, new Vector2(40, 10));
        TextMeshProUGUI timerTxt = CreateText(topBar.transform, "TimerText", "01:00", 48, TextWhite,
            TextAlignmentOptions.MidlineRight, new Vector2(0.6f, 0), new Vector2(1, 1),
            Vector2.zero, Vector2.zero, true, new Vector2(-40, -10));

        // Current word display
        TextMeshProUGUI currentWordTxt = CreateText(panel.transform, "CurrentWordText", "", 56, TextWhite,
            TextAlignmentOptions.Center, new Vector2(0, 1), new Vector2(1, 1),
            Vector2.zero, Vector2.zero, true, new Vector2(0, -160), new Vector2(0, -260));

        // Grid
        GameObject gridObj = new GameObject("GridView");
        gridObj.transform.SetParent(panel.transform, false);
        RectTransform gridRect = gridObj.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.5f, 0.5f); gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.anchoredPosition = new Vector2(0, 30);
        gridRect.sizeDelta = new Vector2(560, 560);

        GridLayoutGroup layout = gridObj.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(120, 120);
        layout.spacing = new Vector2(16, 16);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 4;

        gridView = gridObj.AddComponent<GridView>();
        SerializedObject gridSO = new SerializedObject(gridView);
        gridSO.FindProperty("_tilePrefab").objectReferenceValue = tilePrefab;
        gridSO.FindProperty("_gridParent").objectReferenceValue = gridObj.transform;
        gridSO.FindProperty("_gridLayout").objectReferenceValue = layout;
        gridSO.ApplyModifiedProperties();

        // Found words area
        TextMeshProUGUI wordCountTxt = CreateText(panel.transform, "WordCountText", "Words: 0", 36, TextWhite,
            TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -350), new Vector2(300, 50));
        TextMeshProUGUI foundWordsTxt = CreateText(panel.transform, "FoundWordsText", "", 30, TextGray,
            TextAlignmentOptions.Top, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -430), new Vector2(500, 200));

        // HUD
        GameObject hudObj = new GameObject("HUDView");
        hudObj.transform.SetParent(panel.transform, false);
        hudView = hudObj.AddComponent<HUDView>();
        SerializedObject hudSO = new SerializedObject(hudView);
        hudSO.FindProperty("_scoreText").objectReferenceValue = scoreTxt;
        hudSO.FindProperty("_timerText").objectReferenceValue = timerTxt;
        hudSO.FindProperty("_currentWordText").objectReferenceValue = currentWordTxt;
        hudSO.FindProperty("_foundWordsText").objectReferenceValue = foundWordsTxt;
        hudSO.FindProperty("_wordCountText").objectReferenceValue = wordCountTxt;
        hudSO.ApplyModifiedProperties();

        // Input overlay (transparent, receives raycasts)
        GameObject inputOverlay = new GameObject("InputOverlay");
        inputOverlay.transform.SetParent(panel.transform, false);
        Image overlayImg = inputOverlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0);
        overlayImg.raycastTarget = true;
        RectTransform overlayRect = inputOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero; overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;

        inputManager = inputOverlay.AddComponent<InputManager>();
        SerializedObject inputSO = new SerializedObject(inputManager);
        inputSO.FindProperty("_graphicRaycaster").objectReferenceValue = raycaster;
        inputSO.ApplyModifiedProperties();

        return cg;
    }

    static CanvasGroup BuildGameOverScreen(Transform parent, out GameOverScreen script)
    {
        GameObject panel = CreateScreenPanel(parent, "GameOverScreen");
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        CreateText(panel.transform, "Title", "TIME'S UP!", 72, AccentBlue, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 350), new Vector2(800, 100));

        CreateText(panel.transform, "ScoreLabel", "FINAL SCORE", 36, TextGray, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 200), new Vector2(400, 50));

        TextMeshProUGUI finalScoreTxt = CreateText(panel.transform, "FinalScore", "0", 120, AccentGreen,
            TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 80), new Vector2(600, 150));

        TextMeshProUGUI wordsFoundTxt = CreateText(panel.transform, "WordsFound", "Words Found: 0", 40, TextWhite,
            TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -40), new Vector2(400, 60));

        TextMeshProUGUI newHighScoreTxt = CreateText(panel.transform, "NewHighScore", "NEW HIGH SCORE!", 44,
            new Color(1f, 0.84f, 0f), TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(600, 60));

        Button playAgainBtn = CreateMenuButton(panel.transform, "PlayAgainButton", "PLAY AGAIN", AccentBlue, new Vector2(0, -250));
        Button mainMenuBtn = CreateMenuButton(panel.transform, "MainMenuButton", "MAIN MENU", ButtonBg, new Vector2(0, -370));

        script = panel.AddComponent<GameOverScreen>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("_finalScoreText").objectReferenceValue = finalScoreTxt;
        so.FindProperty("_wordsFoundText").objectReferenceValue = wordsFoundTxt;
        so.FindProperty("_newHighScoreText").objectReferenceValue = newHighScoreTxt;
        so.FindProperty("_playAgainButton").objectReferenceValue = playAgainBtn;
        so.FindProperty("_mainMenuButton").objectReferenceValue = mainMenuBtn;
        so.ApplyModifiedProperties();

        return cg;
    }

    static CanvasGroup BuildLeaderboardScreen(Transform parent, GameObject entryPrefab, out LeaderboardView lbView)
    {
        GameObject panel = CreateScreenPanel(parent, "LeaderboardScreen");
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        CreateText(panel.transform, "Title", "LEADERBOARD", 60, AccentBlue, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(800, 80));

        // Entries container
        GameObject entriesParent = new GameObject("Entries");
        entriesParent.transform.SetParent(panel.transform, false);
        RectTransform entriesRect = entriesParent.AddComponent<RectTransform>();
        entriesRect.anchorMin = new Vector2(0.1f, 0.2f); entriesRect.anchorMax = new Vector2(0.9f, 0.85f);
        entriesRect.sizeDelta = Vector2.zero;
        VerticalLayoutGroup vlg = entriesParent.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12;
        vlg.childControlWidth = true; vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

        Button backBtn = CreateBackButton(panel.transform);

        lbView = panel.AddComponent<LeaderboardView>();
        SerializedObject so = new SerializedObject(lbView);
        so.FindProperty("_entriesParent").objectReferenceValue = entriesParent.transform;
        so.FindProperty("_entryPrefab").objectReferenceValue = entryPrefab;
        so.FindProperty("_backButton").objectReferenceValue = backBtn;
        so.ApplyModifiedProperties();

        return cg;
    }

    static CanvasGroup BuildHowToPlayScreen(Transform parent, out HowToPlayScreen script)
    {
        GameObject panel = CreateScreenPanel(parent, "HowToPlayScreen");
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        CreateText(panel.transform, "Title", "HOW TO PLAY", 60, AccentBlue, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(800, 80));

        string instructions =
            "<b>Swipe adjacent letters</b> to form words.\n\n" +
            "Words must be <b>3+ letters</b> long.\n\n" +
            "<b>Scoring:</b>\n" +
            "  3 letters = 100 pts\n" +
            "  4 letters = 400 pts\n" +
            "  5 letters = 800 pts\n" +
            "  6 letters = 1,200 pts\n" +
            "  7+ letters = 2,000 pts\n\n" +
            "<b>Speed Bonus:</b>\n" +
            "  Under 5 sec = 1.5× multiplier\n" +
            "  Under 10 sec = 1.25× multiplier\n\n" +
            "Each word can only be scored <b>once</b> per round.\n\n" +
            "Green tiles = valid prefix\n" +
            "Red tiles = invalid prefix";

        CreateText(panel.transform, "Instructions", instructions, 34, TextWhite, TextAlignmentOptions.TopLeft,
            new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.85f), Vector2.zero, Vector2.zero, true);

        Button backBtn = CreateBackButton(panel.transform);

        script = panel.AddComponent<HowToPlayScreen>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("_backButton").objectReferenceValue = backBtn;
        so.ApplyModifiedProperties();

        return cg;
    }

    static CanvasGroup BuildSettingsScreen(Transform parent, out SettingsScreen script)
    {
        GameObject panel = CreateScreenPanel(parent, "SettingsScreen");
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        CreateText(panel.transform, "Title", "SETTINGS", 60, AccentBlue, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(600, 80));

        // Grid Size section
        CreateText(panel.transform, "GridLabel", "Grid Size", 40, TextWhite, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 280), new Vector2(300, 60));

        Button g3 = CreateOptionButton(panel.transform, "Grid3x3", "3×3", new Vector2(-200, 200), out Image g3Bg);
        Button g4 = CreateOptionButton(panel.transform, "Grid4x4", "4×4", new Vector2(0, 200), out Image g4Bg);
        Button g5 = CreateOptionButton(panel.transform, "Grid5x5", "5×5", new Vector2(200, 200), out Image g5Bg);

        // Duration section
        CreateText(panel.transform, "DurationLabel", "Round Duration", 40, TextWhite, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(400, 60));

        Button d30 = CreateOptionButton(panel.transform, "Dur30", "30s", new Vector2(-270, -20), out Image d30Bg);
        Button d60 = CreateOptionButton(panel.transform, "Dur60", "60s", new Vector2(-90, -20), out Image d60Bg);
        Button d90 = CreateOptionButton(panel.transform, "Dur90", "90s", new Vector2(90, -20), out Image d90Bg);
        Button d120 = CreateOptionButton(panel.transform, "Dur120", "120s", new Vector2(270, -20), out Image d120Bg);

        Button backBtn = CreateBackButton(panel.transform);

        script = panel.AddComponent<SettingsScreen>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("_grid3x3Button").objectReferenceValue = g3;
        so.FindProperty("_grid4x4Button").objectReferenceValue = g4;
        so.FindProperty("_grid5x5Button").objectReferenceValue = g5;
        so.FindProperty("_grid3x3Bg").objectReferenceValue = g3Bg;
        so.FindProperty("_grid4x4Bg").objectReferenceValue = g4Bg;
        so.FindProperty("_grid5x5Bg").objectReferenceValue = g5Bg;
        so.FindProperty("_duration30Button").objectReferenceValue = d30;
        so.FindProperty("_duration60Button").objectReferenceValue = d60;
        so.FindProperty("_duration90Button").objectReferenceValue = d90;
        so.FindProperty("_duration120Button").objectReferenceValue = d120;
        so.FindProperty("_duration30Bg").objectReferenceValue = d30Bg;
        so.FindProperty("_duration60Bg").objectReferenceValue = d60Bg;
        so.FindProperty("_duration90Bg").objectReferenceValue = d90Bg;
        so.FindProperty("_duration120Bg").objectReferenceValue = d120Bg;
        so.FindProperty("_backButton").objectReferenceValue = backBtn;
        so.ApplyModifiedProperties();

        return cg;
    }

    // ================================================================
    //  HELPER METHODS
    // ================================================================

    static GameObject CreateScreenPanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = BgDark;
        bg.raycastTarget = true;

        CanvasGroup cg = panel.AddComponent<CanvasGroup>();
        return panel;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize,
        Color color, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta, bool stretch = false, Vector2 offsetMin = default, Vector2 offsetMax = default)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        tmp.richText = true;
        tmp.raycastTarget = false;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;

        if (stretch)
        {
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }
        else
        {
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }

        return tmp;
    }

    static Button CreateBackButton(Transform parent)
    {
        GameObject btnObj = new GameObject("BackButton");
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // top-left
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -20);
        rt.sizeDelta = new Vector2(180, 65);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.22f, 0.28f, 0.85f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.32f, 0.38f);
        colors.pressedColor = new Color(0.15f, 0.17f, 0.22f);
        btn.colors = colors;

        // Arrow + "Back" text
        CreateText(btnObj.transform, "Label", "<  Back", 28, TextWhite, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);

        return btn;
    }

    static Button CreateMenuButton(Transform parent, string name, string label, Color bgColor, Vector2 pos)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(550, 90);

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.1f, bgColor.b + 0.1f);
        colors.pressedColor = new Color(bgColor.r - 0.05f, bgColor.g - 0.05f, bgColor.b - 0.05f);
        btn.colors = colors;

        CreateText(btnObj.transform, "Label", label, 40, TextWhite, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);

        return btn;
    }

    static Button CreateOptionButton(Transform parent, string name, string label, Vector2 pos, out Image bgImage)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(150, 70);

        bgImage = btnObj.AddComponent<Image>();
        bgImage.color = ButtonBg;

        Button btn = btnObj.AddComponent<Button>();

        CreateText(btnObj.transform, "Label", label, 34, TextWhite, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);

        return btn;
    }

    static Button CreateSmallButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos; rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0); // transparent

        Button btn = btnObj.AddComponent<Button>();

        CreateText(btnObj.transform, "Label", label, 36, TextWhite, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);

        return btn;
    }

    static GameObject CreateTilePrefab()
    {
        GameObject tileObj = new GameObject("TilePrefab");
        Image tileImage = tileObj.AddComponent<Image>();
        tileImage.color = TileColor;
        tileObj.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);

        GameObject textChild = new GameObject("LetterText");
        textChild.transform.SetParent(tileObj.transform, false);
        TextMeshProUGUI tmp = textChild.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.1f, 0.1f, 0.1f);
        tmp.fontSize = 56;
        tmp.fontStyle = FontStyles.Bold;
        RectTransform textRect = textChild.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.sizeDelta = Vector2.zero;

        Tile tileScript = tileObj.AddComponent<Tile>();
        SerializedObject tileSO = new SerializedObject(tileScript);
        tileSO.FindProperty("_letterText").objectReferenceValue = tmp;
        tileSO.FindProperty("_backgroundImage").objectReferenceValue = tileImage;
        tileSO.ApplyModifiedProperties();

        return tileObj;
    }

    static GameObject CreateLeaderboardEntryPrefab()
    {
        GameObject entryObj = new GameObject("LeaderboardEntry");
        RectTransform rt = entryObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 60);

        Image bg = entryObj.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.20f, 0.26f);

        HorizontalLayoutGroup hlg = entryObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.padding = new RectOffset(20, 20, 5, 5);
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;

        // Rank
        GameObject rankObj = new GameObject("Rank");
        rankObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI rankTxt = rankObj.AddComponent<TextMeshProUGUI>();
        rankTxt.text = "#1"; rankTxt.fontSize = 32; rankTxt.color = AccentBlue;
        rankTxt.alignment = TextAlignmentOptions.MidlineLeft;
        LayoutElement rankLE = rankObj.AddComponent<LayoutElement>();
        rankLE.preferredWidth = 60;

        // Name
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI nameTxt = nameObj.AddComponent<TextMeshProUGUI>();
        nameTxt.text = "Player"; nameTxt.fontSize = 32; nameTxt.color = TextWhite;
        nameTxt.alignment = TextAlignmentOptions.MidlineLeft;
        LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
        nameLE.flexibleWidth = 1;

        // Score
        GameObject scoreObj = new GameObject("Score");
        scoreObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI scoreTxt = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreTxt.text = "0"; scoreTxt.fontSize = 32; scoreTxt.color = AccentGreen;
        scoreTxt.alignment = TextAlignmentOptions.MidlineRight;
        LayoutElement scoreLE = scoreObj.AddComponent<LayoutElement>();
        scoreLE.preferredWidth = 120;

        return entryObj;
    }
}
