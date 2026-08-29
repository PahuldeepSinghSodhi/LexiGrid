using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LexiGrid.DataStructures;
using LexiGrid.UI;

namespace LexiGrid.Core
{
    /// <summary>
    /// Represents the different states of the game.
    /// </summary>
    public enum GameState
    {
        Splash,
        MainMenu,
        Playing,
        GameOver
    }

    /// <summary>
    /// The central GameManager — drives the state machine and connects all systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("UI Views")]
        [SerializeField] private ScreenManager _screenManager;
        [SerializeField] private GridView _gridView;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private HUDView _hudView;
        [SerializeField] private LeaderboardView _leaderboardView;
        [SerializeField] private SplashScreen _splashScreen;
        [SerializeField] private GameOverScreen _gameOverScreen;

        [Header("Data")]
        [SerializeField] private TextAsset _wordListAsset;

        [Header("Defaults (overridden by Settings)")]
        [SerializeField] private int _gridSize = 4;
        [SerializeField] private float _roundDuration = 60f;
        [SerializeField] private int _leaderboardMaxSize = 10;

        private GameState _currentState;
        private DictionaryManager _dictionaryManager;
        private BoardGenerator _boardGenerator;
        private MinHeap<LeaderboardEntry> _leaderboard;
        private int _currentScore;
        private float _timeRemaining;
        private float _lastWordTime;
        private HashSet<string> _foundWordsThisRound;
        private int _wordsFoundThisRound;

        public GameState CurrentState => _currentState;
        public int CurrentScore => _currentScore;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _dictionaryManager = new DictionaryManager();
            _leaderboard = PersistenceManager.LoadLeaderboard();
            _foundWordsThisRound = new HashSet<string>();

            TransitionTo(GameState.Splash);
        }

        private void Update()
        {
            if (_currentState == GameState.Playing)
            {
                _timeRemaining -= Time.deltaTime;

                if (_hudView != null)
                {
                    _hudView.UpdateTimer(_timeRemaining);
                }

                if (_timeRemaining <= 0)
                {
                    _timeRemaining = 0;
                    TransitionTo(GameState.GameOver);
                }
            }
        }

        /// <summary>
        /// Transitions the game to a new state and updates screens.
        /// </summary>
        public void TransitionTo(GameState newState)
        {
            _currentState = newState;

            switch (_currentState)
            {
                case GameState.Splash:
                    OnEnterSplash();
                    break;
                case GameState.MainMenu:
                    OnEnterMainMenu();
                    break;
                case GameState.Playing:
                    break;
                case GameState.GameOver:
                    OnGameOver();
                    break;
            }
        }

        // ==================== SPLASH ====================

        private void OnEnterSplash()
        {
            _screenManager?.ShowScreenImmediate(_screenManager.SplashScreen);

            if (_wordListAsset != null)
            {
                StartCoroutine(LoadDictionaryCoroutine());
            }
            else
            {
                Debug.LogWarning("No word list asset assigned.");
                TransitionTo(GameState.MainMenu);
            }
        }

        private IEnumerator LoadDictionaryCoroutine()
        {
            yield return _dictionaryManager.LoadFromTextAssetAsync(_wordListAsset, (progress) =>
            {
                _splashScreen?.UpdateProgress(progress);
            });

            _splashScreen?.OnLoadingComplete();
            Debug.Log($"Dictionary loaded with {_dictionaryManager.WordCount} words.");

            // Brief pause to show "Ready!" text
            yield return new WaitForSeconds(0.5f);

            TransitionTo(GameState.MainMenu);
        }

        // ==================== MAIN MENU ====================

        private void OnEnterMainMenu()
        {
            _screenManager?.FadeToScreen(_screenManager.MainMenuScreen);
        }

        // ==================== GAME ====================

        /// <summary>
        /// Starts a new round. Reads grid size and duration from Settings (PlayerPrefs).
        /// </summary>
        public void StartNewRound()
        {
            // Read settings
            _gridSize = SettingsScreen.GetSavedGridSize();
            _roundDuration = SettingsScreen.GetSavedDuration();

            _boardGenerator = new BoardGenerator(_gridSize);
            _currentScore = 0;
            _timeRemaining = _roundDuration;
            _foundWordsThisRound.Clear();
            _wordsFoundThisRound = 0;
            _lastWordTime = Time.time;

            if (_hudView != null)
            {
                _hudView.Reset();
                _hudView.UpdateScore(_currentScore);
                _hudView.UpdateTimer(_timeRemaining);
            }

            char[,] board = _boardGenerator.GenerateSolvableBoard(_dictionaryManager.Dictionary);

            if (_gridView != null)
            {
                _gridView.Initialize(_gridSize);
                _gridView.PopulateGrid(board);
            }

            if (_inputManager != null)
            {
                _inputManager.Initialize(_dictionaryManager.Dictionary);
                _inputManager.OnWordSubmitted += OnWordSubmitted;
            }

            _screenManager?.FadeToScreen(_screenManager.GameScreen);
            _currentState = GameState.Playing;
        }

        private void OnWordSubmitted(string word)
        {
            if (_currentState != GameState.Playing) return;

            string lowerWord = word.ToLowerInvariant();
            if (_foundWordsThisRound.Contains(lowerWord)) return;

            float timeSinceLastWord = Time.time - _lastWordTime;
            int score = ScoringEngine.CalculateFinalScore(word.Length, timeSinceLastWord);

            if (score <= 0) return;

            _currentScore += score;
            _foundWordsThisRound.Add(lowerWord);
            _wordsFoundThisRound++;
            _lastWordTime = Time.time;

            if (_hudView != null)
            {
                _hudView.UpdateScore(_currentScore);
                _hudView.AddFoundWord(word, score);
            }
        }

        // ==================== GAME OVER ====================

        private void OnGameOver()
        {
            if (_inputManager != null)
            {
                _inputManager.OnWordSubmitted -= OnWordSubmitted;
            }

            // Save to leaderboard
            string playerName = "Player";
            LeaderboardEntry newEntry = new LeaderboardEntry(playerName, _currentScore);
            bool isNewHighScore = _leaderboard.TryInsertIfBetter(newEntry, _leaderboardMaxSize);
            PersistenceManager.SaveLeaderboard(_leaderboard);

            // Show Game Over screen
            _gameOverScreen?.ShowResults(_currentScore, _wordsFoundThisRound, isNewHighScore);
            _screenManager?.FadeToScreen(_screenManager.GameOverScreen);
        }

        // ==================== NAVIGATION ====================

        /// <summary>
        /// Returns to the main menu from any screen.
        /// </summary>
        public void ReturnToMenu()
        {
            // Clean up if we were playing
            if (_inputManager != null)
            {
                _inputManager.OnWordSubmitted -= OnWordSubmitted;
            }
            TransitionTo(GameState.MainMenu);
        }

        /// <summary>
        /// Shows the leaderboard screen.
        /// </summary>
        public void ShowLeaderboardScreen()
        {
            if (_leaderboardView != null)
            {
                LeaderboardEntry[] sortedEntries = _leaderboard.GetSortedDescending();
                _leaderboardView.DisplayLeaderboard(sortedEntries);
            }
            _screenManager?.FadeToScreen(_screenManager.LeaderboardScreen);
        }

        /// <summary>
        /// Shows the How to Play screen.
        /// </summary>
        public void ShowHowToPlayScreen()
        {
            _screenManager?.FadeToScreen(_screenManager.HowToPlayScreen);
        }

        /// <summary>
        /// Shows the Settings screen.
        /// </summary>
        public void ShowSettingsScreen()
        {
            _screenManager?.FadeToScreen(_screenManager.SettingsScreen);
        }

        /// <summary>
        /// Returns the best score from the leaderboard.
        /// </summary>
        public int GetBestScore()
        {
            if (_leaderboard == null || _leaderboard.IsEmpty) return 0;
            var sorted = _leaderboard.GetSortedDescending();
            return sorted.Length > 0 ? sorted[0].Score : 0;
        }
    }
}
