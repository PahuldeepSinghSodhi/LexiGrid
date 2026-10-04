using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using LexiGrid.Core;
using LexiGrid.UI;
using GameInput = LexiGrid.UI.InputManager;

namespace LexiGrid.Editor
{
    /// <summary>Creates the missing scene and prefabs reproducibly using the original game systems.</summary>
    public static class LexiGridBuild
    {
        private static TMP_FontAsset _font;
        private static Sprite _rounded;
        private static readonly Color Ink = new Color(.035f, .055f, .09f);
        private static readonly Color Panel = new Color(.075f, .105f, .155f);
        private static readonly Color Muted = new Color(.56f, .65f, .75f);
        private static readonly Color Mint = new Color(.40f, .92f, .76f);

        [MenuItem("LexiGrid/Create or rebuild playable scene")]
        public static void CreateScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Generated");
            ImportTextResources();
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (_font == null) throw new Exception("TextMesh Pro essential font resources missing.");
            _rounded = CreateRoundedSprite();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Camera", typeof(Camera));
            camera.tag = "MainCamera";
            camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            camera.GetComponent<Camera>().backgroundColor = Ink;
            var canvasGO = new GameObject("LexiGrid UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 900);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            var responsive = canvasGO.AddComponent<ResponsiveInterface>();
            responsive.Scaler = scaler;
            var background = Box("Background", canvasGO.transform, Vector2.zero, Vector2.zero, Ink, false);
            Stretch(background.rectTransform);
            var screens = canvasGO.AddComponent<ScreenManager>();
            var manager = new GameObject("GameManager").AddComponent<GameManager>();
            var splash = Screen("Splash", canvasGO.transform);
            var menu = Screen("MainMenu", canvasGO.transform);
            var game = Screen("Game", canvasGO.transform);
            var over = Screen("GameOver", canvasGO.transform);
            var leaderboard = Screen("Leaderboard", canvasGO.transform);
            var rules = Screen("HowToPlay", canvasGO.transform);
            var settings = Screen("Settings", canvasGO.transform);
            Ref(screens, "_splashScreen", splash); Ref(screens, "_mainMenuScreen", menu);
            Ref(screens, "_gameScreen", game); Ref(screens, "_gameOverScreen", over);
            Ref(screens, "_leaderboardScreen", leaderboard); Ref(screens, "_howToPlayScreen", rules); Ref(screens, "_settingsScreen", settings);

            // Splash and loading UI.
            Text(splash.transform, "LEXIGRID", new Vector2(0, 95), new Vector2(650, 110), 80, Color.white);
            Text(splash.transform, "A little focus. A lot of possibilities.", new Vector2(0, 5), new Vector2(700, 60), 25, Muted);
            var loading = splash.gameObject.AddComponent<SplashScreen>();
            Ref(loading, "_loadingText", Text(splash.transform, "Preparing your dictionary...", new Vector2(0, -100), new Vector2(700, 60), 24, Mint));
            var track = Box("Loading track", splash.transform, new Vector2(0, -145), new Vector2(470, 10), Panel, false);
            var fill = Box("Loading fill", track.transform, Vector2.zero, Vector2.zero, Mint, false); Stretch(fill.rectTransform);
            var slider = track.gameObject.AddComponent<Slider>(); slider.fillRect = fill.rectTransform; slider.interactable = false;
            Ref(loading, "_loadingBar", slider);

            // Main menu, with a decorative high-resolution letter preview.
            var preview = Rect("Letter illustration", menu.transform, new Vector2(-330, 10), new Vector2(560, 560));
            responsive.MenuPreview = preview;
            Text(preview, "FIND YOUR FLOW", new Vector2(0, 275), new Vector2(550, 45), 19, Mint);
            string letters = "LEXIGRIDWORDPLAY";
            for (int i = 0; i < letters.Length; i++)
            {
                bool accented = i < 4;
                var tile = Box("Preview tile", preview, new Vector2(-195 + (i % 4) * 130, 170 - (i / 4) * 130), new Vector2(115, 115), accented ? Mint : Panel, false);
                Text(tile.transform, letters[i].ToString(), Vector2.zero, new Vector2(110, 110), 57, accented ? Ink : Color.white);
            }
            var copy = Rect("Menu content", menu.transform, new Vector2(330, 0), new Vector2(560, 640));
            responsive.MenuCopy = copy;
            Text(copy, "THE EVERYDAY WORD CHALLENGE", new Vector2(0, 260), new Vector2(560, 40), 18, Mint);
            Text(copy, "LexiGrid", new Vector2(0, 165), new Vector2(560, 140), 92, Color.white);
            Text(copy, "Connect letters. Discover words.\nMake every second count.", new Vector2(0, 55), new Vector2(560, 95), 27, Muted);
            var main = menu.gameObject.AddComponent<MainMenuScreen>();
            Ref(main, "_playButton", Button(copy, "Play a round", new Vector2(0, -55), new Vector2(470, 72), true));
            Ref(main, "_leaderboardButton", Button(copy, "Leaderboard", new Vector2(-122, -145), new Vector2(226, 60)));
            Ref(main, "_settingsButton", Button(copy, "Settings", new Vector2(122, -145), new Vector2(226, 60)));
            Ref(main, "_howToPlayButton", Button(copy, "How to play", new Vector2(0, -222), new Vector2(470, 52)));
            Ref(main, "_bestScoreText", Text(copy, "", new Vector2(0, -295), new Vector2(520, 40), 20, Mint));

            // Game board and HUD: square tiles, scalable SDF text, and a side panel.
            Text(game.transform, "LexiGrid", new Vector2(-470, 0), new Vector2(300, 65), 42, Color.white).rectTransform.anchorMin = new Vector2(.5f, .93f);
            var heading = game.transform.GetChild(game.transform.childCount - 1) as RectTransform; heading.anchorMax = heading.anchorMin;
            var exit = Button(game.transform, "Main menu", new Vector2(470, 0), new Vector2(175, 48));
            var exitRect = exit.transform as RectTransform; exitRect.anchorMin = exitRect.anchorMax = new Vector2(.5f, .93f);
            UnityEventTools.AddPersistentListener(exit.onClick, manager.ReturnToMenu);
            var board = Rect("Board", game.transform, new Vector2(-230, -25), new Vector2(580, 580));
            var boardImage = board.gameObject.AddComponent<Image>(); boardImage.color = Panel; boardImage.sprite = _rounded; boardImage.type = Image.Type.Sliced;
            var layout = board.gameObject.AddComponent<GridLayoutGroup>(); layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4; layout.spacing = new Vector2(12, 12); layout.padding = new RectOffset(16, 16, 16, 16); layout.childAlignment = TextAnchor.MiddleCenter;
            var grid = board.gameObject.AddComponent<GridView>();
            var input = board.gameObject.AddComponent<GameInput>();
            Ref(input, "_graphicRaycaster", canvasGO.GetComponent<GraphicRaycaster>());
            Ref(grid, "_gridParent", board); Ref(grid, "_gridLayout", layout); Ref(grid, "_tilePrefab", CreateTilePrefab());
            var sidebar = Box("Round details", game.transform, new Vector2(360, -25), new Vector2(390, 580), Panel, false);
            responsive.Board = board; responsive.Sidebar = sidebar.rectTransform;
            var hud = game.gameObject.AddComponent<HUDView>();
            var summary = Rect("Summary", sidebar.transform, Vector2.zero, Vector2.zero); Stretch(summary);
            Ref(hud, "_scoreText", AnchoredText(summary, "Score: 0", new Vector2(.5f, .86f), new Vector2(330, 50), 34, Color.white));
            Ref(hud, "_timerText", AnchoredText(summary, "01:00", new Vector2(.5f, .70f), new Vector2(330, 75), 62, Color.white));
            Ref(hud, "_wordCountText", AnchoredText(summary, "Words: 0", new Vector2(.5f, .55f), new Vector2(330, 40), 22, Mint));
            Ref(hud, "_foundWordsText", AnchoredText(summary, "", new Vector2(.5f, .29f), new Vector2(330, 200), 23, Color.white));
            var path = AnchoredText(game.transform, "Drag across letters to build a word", new Vector2(.5f, .10f), new Vector2(850, 60), 28, Mint);
            responsive.Path = path.rectTransform;
            Ref(hud, "_currentWordText", path);
            var controls = Rect("Selection controls", game.transform, new Vector2(0, -418), new Vector2(850, 48));
            responsive.Controls = controls;
            UnityEventTools.AddPersistentListener(Button(controls, "Submit", new Vector2(-210, 0), new Vector2(190, 44), true).onClick, input.SubmitSelection);
            UnityEventTools.AddPersistentListener(Button(controls, "Undo letter", Vector2.zero, new Vector2(190, 44)).onClick, input.UndoLastSelection);
            UnityEventTools.AddPersistentListener(Button(controls, "Clear", new Vector2(210, 0), new Vector2(190, 44)).onClick, input.CancelSelection);

            // Results.
            Text(over.transform, "ROUND COMPLETE", new Vector2(0, 260), new Vector2(700, 45), 20, Mint);
            Text(over.transform, "Nice word work.", new Vector2(0, 185), new Vector2(800, 90), 60, Color.white);
            var result = over.gameObject.AddComponent<GameOverScreen>();
            Ref(result, "_finalScoreText", Text(over.transform, "0", new Vector2(0, 65), new Vector2(800, 120), 92, Color.white));
            Ref(result, "_wordsFoundText", Text(over.transform, "Words Found: 0", new Vector2(0, -35), new Vector2(600, 50), 26, Muted));
            Ref(result, "_newHighScoreText", Text(over.transform, "New personal best!", new Vector2(0, -90), new Vector2(600, 40), 24, Mint));
            Ref(result, "_playAgainButton", Button(over.transform, "Play again", new Vector2(0, -185), new Vector2(440, 70), true));
            Ref(result, "_mainMenuButton", Button(over.transform, "Main menu", new Vector2(0, -272), new Vector2(440, 60)));

            // Settings preserve the existing size and duration choices.
            Text(settings.transform, "Make it your game.", new Vector2(0, 280), new Vector2(900, 100), 56, Color.white);
            Text(settings.transform, "BOARD SIZE", new Vector2(0, 170), new Vector2(600, 40), 20, Mint);
            var preferences = settings.gameObject.AddComponent<SettingsScreen>();
            for (int i = 0; i < 3; i++)
            {
                int size = i + 3;
                var button = Button(settings.transform, size + " x " + size, new Vector2(-210 + i * 210, 90), new Vector2(185, 70));
                Ref(preferences, "_grid" + size + "x" + size + "Button", button);
                Ref(preferences, "_grid" + size + "x" + size + "Bg", button.GetComponent<Image>());
            }
            Text(settings.transform, "ROUND LENGTH", new Vector2(0, -20), new Vector2(600, 40), 20, Mint);
            int[] durations = {30, 60, 90, 120};
            for (int i = 0; i < durations.Length; i++)
            {
                var button = Button(settings.transform, durations[i] + " sec", new Vector2(-270 + i * 180, -100), new Vector2(160, 70));
                Ref(preferences, "_duration" + durations[i] + "Button", button);
                Ref(preferences, "_duration" + durations[i] + "Bg", button.GetComponent<Image>());
            }
            Text(settings.transform, "Your choices are saved automatically.", new Vector2(0, -215), new Vector2(800, 40), 21, Muted);
            Ref(preferences, "_backButton", Button(settings.transform, "Done", new Vector2(0, -295), new Vector2(420, 64), true));

            Text(rules.transform, "A few simple rules.", new Vector2(0, 270), new Vector2(900, 100), 56, Color.white);
            Text(rules.transform, "01   Drag neighbours, or tap tiles then press Submit.\n02   Tap a selected tile to remove it and later letters.\n03   Retrace while holding to undo; keep the anchor.\n04   Use 3+ letters. Each word scores once per round.\n05   Two valid words under 5s apart earn +2s per pair.\n06   Invalid or repeated submissions break the pair.", new Vector2(0, 60), new Vector2(790, 310), 25, Color.white, TextAlignmentOptions.Left);
            Text(rules.transform, "Base points: 100 / 400 / 800 / 1,200 / 2,000\nFind a word under 5s: x1.5  |  5 to under 10s: x1.25", new Vector2(0, -160), new Vector2(850, 100), 22, Mint);
            Ref(rules.gameObject.AddComponent<HowToPlayScreen>(), "_backButton", Button(rules.transform, "Let's play", new Vector2(0, -295), new Vector2(420, 64), true));

            Text(leaderboard.transform, "Your best rounds.", new Vector2(0, 340), new Vector2(900, 90), 56, Color.white);
            Text(leaderboard.transform, "TOP 10  /  SAVED ON THIS DEVICE", new Vector2(0, 265), new Vector2(900, 45), 19, Mint);
            var entries = Rect("Entries", leaderboard.transform, new Vector2(0, -15), new Vector2(720, 480));
            var list = entries.gameObject.AddComponent<VerticalLayoutGroup>(); list.spacing = 6; list.childControlHeight = true; list.childControlWidth = true; list.childForceExpandHeight = false;
            var ranks = leaderboard.gameObject.AddComponent<LeaderboardView>();
            Ref(ranks, "_entriesParent", entries); Ref(ranks, "_entryPrefab", CreateEntryPrefab());
            Ref(ranks, "_backButton", Button(leaderboard.transform, "Main menu", new Vector2(0, -330), new Vector2(420, 60)));

            Ref(manager, "_screenManager", screens); Ref(manager, "_gridView", grid); Ref(manager, "_inputManager", input);
            Ref(manager, "_hudView", hud); Ref(manager, "_leaderboardView", ranks); Ref(manager, "_splashScreen", loading); Ref(manager, "_gameOverScreen", result);
            Ref(manager, "_wordListAsset", AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/wordlist.txt"));
            foreach (var group in new[] {splash, menu, game, over, leaderboard, rules, settings}) group.gameObject.SetActive(false);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/LexiGrid.unity");
            EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene("Assets/Scenes/LexiGrid.unity", true)};
            Debug.Log("LexiGrid scene created with all game systems wired.");
        }

        [MenuItem("LexiGrid/Build Windows player")]
        public static void BuildWindows()
        {
            CreateScene();
            PlayerSettings.companyName = "LexiGrid"; PlayerSettings.productName = "LexiGrid";
            PlayerSettings.defaultScreenWidth = 1440; PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true;
            PlayerSettings.allowFullscreenSwitch = true; PlayerSettings.runInBackground = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            QualitySettings.vSyncCount = 1;
            string destination = Path.GetFullPath("../Builds/Windows/LexiGrid.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes = new[] {"Assets/Scenes/LexiGrid.unity"}, locationPathName = destination, target = BuildTarget.StandaloneWindows64, options = BuildOptions.None});
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Build failed: " + report.summary.result);
            Debug.Log("LEXIGRID_BUILD_SUCCESS " + destination);
        }

        private static void ImportTextResources()
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset") != null) return;
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui/package.json");
            string file = package == null ? "" : Path.Combine(package.resolvedPath, "Package Resources/TMP Essential Resources.unitypackage");
            if (File.Exists(file)) { AssetDatabase.ImportPackage(file, false); AssetDatabase.Refresh(); return; }
            throw new Exception("TMP essential resources package not found in uGUI.");
        }

        private static Sprite CreateRoundedSprite()
        {
            string path = "Assets/Generated/Rounded.png";
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x - 31.5f) - 19.5f, 0), dy = Mathf.Max(Mathf.Abs(y - 31.5f) - 19.5f, 0);
                float alpha = Mathf.Clamp01(12.5f - Mathf.Sqrt(dx * dx + dy * dy));
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
            texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite;
            importer.spriteBorder = new Vector4(16, 16, 16, 16); importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreateTilePrefab()
        {
            var tile = Box("LetterTile", null, Vector2.zero, new Vector2(120, 120), Panel);
            var text = Text(tile.transform, "A", Vector2.zero, new Vector2(110, 110), 60, Color.white);
            Stretch(text.rectTransform); text.margin = new Vector4(8, 8, 8, 8); text.enableAutoSizing = true; text.fontSizeMin = 24; text.fontSizeMax = 65;
            var logic = tile.gameObject.AddComponent<Tile>(); Ref(logic, "_letterText", text); Ref(logic, "_backgroundImage", tile);
            var prefab = PrefabUtility.SaveAsPrefabAsset(tile.gameObject, "Assets/Generated/LetterTile.prefab"); UnityEngine.Object.DestroyImmediate(tile.gameObject); return prefab;
        }

        private static GameObject CreateEntryPrefab()
        {
            var row = Box("LeaderboardRow", null, Vector2.zero, new Vector2(720, 41), Panel, false);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 41;
            Text(row.transform, "#1", new Vector2(-285, 0), new Vector2(75, 40), 20, Mint);
            Text(row.transform, "Player", Vector2.zero, new Vector2(310, 40), 21, Color.white);
            Text(row.transform, "0", new Vector2(270, 0), new Vector2(135, 40), 21, Color.white);
            var prefab = PrefabUtility.SaveAsPrefabAsset(row.gameObject, "Assets/Generated/LeaderboardRow.prefab"); UnityEngine.Object.DestroyImmediate(row.gameObject); return prefab;
        }

        private static CanvasGroup Screen(string name, Transform parent)
        { var rect = Rect(name, parent, Vector2.zero, Vector2.zero); Stretch(rect); return rect.gameObject.AddComponent<CanvasGroup>(); }
        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        private static void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static Image Box(string name, Transform parent, Vector2 position, Vector2 size, Color color, bool raycast = true)
        {
            var rect = Rect(name, parent, position, size); var image = rect.gameObject.AddComponent<Image>(); image.sprite = _rounded;
            image.type = Image.Type.Sliced; image.color = color; image.raycastTarget = raycast; return image;
        }
        private static TextMeshProUGUI Text(Transform parent, string value, Vector2 position, Vector2 size, float fontSize, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var rect = Rect("Text " + value.Split('\n')[0], parent, position, size); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = _font; text.text = value; text.fontSize = fontSize; text.color = color; text.alignment = align;
            text.raycastTarget = false; text.overflowMode = TextOverflowModes.Ellipsis; return text;
        }
        private static TextMeshProUGUI AnchoredText(Transform parent, string value, Vector2 anchor, Vector2 size, float fontSize, Color color)
        { var text = Text(parent, value, Vector2.zero, size, fontSize, color); text.rectTransform.anchorMin = text.rectTransform.anchorMax = anchor; return text; }
        private static Button Button(Transform parent, string label, Vector2 position, Vector2 size, bool primary = false)
        {
            var image = Box(label, parent, position, size, primary ? Mint : Panel); var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image; var colors = button.colors; colors.highlightedColor = new Color(.85f, 1f, .95f); colors.pressedColor = new Color(.65f, .8f, .8f); button.colors = colors;
            var text = Text(image.transform, label, Vector2.zero, size - new Vector2(15, 5), 24, primary ? Ink : Color.white); Stretch(text.rectTransform); return button;
        }
        private static void Ref(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target); var property = serialized.FindProperty(field);
            if (property == null) throw new Exception(target.name + " missing " + field);
            property.objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
