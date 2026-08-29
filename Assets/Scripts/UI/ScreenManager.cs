using UnityEngine;
using System.Collections;

namespace LexiGrid.UI
{
    /// <summary>
    /// Central screen controller — manages showing/hiding screens with fade transitions.
    /// Each screen is a CanvasGroup so we can animate alpha for smooth fades.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private CanvasGroup _splashScreen;
        [SerializeField] private CanvasGroup _mainMenuScreen;
        [SerializeField] private CanvasGroup _gameScreen;
        [SerializeField] private CanvasGroup _gameOverScreen;
        [SerializeField] private CanvasGroup _leaderboardScreen;
        [SerializeField] private CanvasGroup _howToPlayScreen;
        [SerializeField] private CanvasGroup _settingsScreen;

        [Header("Transition")]
        [SerializeField] private float _fadeDuration = 0.3f;

        private CanvasGroup _currentScreen;
        private Coroutine _transitionCoroutine;

        /// <summary>
        /// Reference accessors for external wiring.
        /// </summary>
        public CanvasGroup SplashScreen => _splashScreen;
        public CanvasGroup MainMenuScreen => _mainMenuScreen;
        public CanvasGroup GameScreen => _gameScreen;
        public CanvasGroup GameOverScreen => _gameOverScreen;
        public CanvasGroup LeaderboardScreen => _leaderboardScreen;
        public CanvasGroup HowToPlayScreen => _howToPlayScreen;
        public CanvasGroup SettingsScreen => _settingsScreen;

        private void Awake()
        {
            // Start with everything hidden
            HideAllImmediate();
        }

        /// <summary>
        /// Immediately show a screen with no animation. Hides all others.
        /// </summary>
        public void ShowScreenImmediate(CanvasGroup screen)
        {
            HideAllImmediate();
            if (screen != null)
            {
                SetScreenActive(screen, true);
                _currentScreen = screen;
            }
        }

        /// <summary>
        /// Transition to a screen with a fade animation.
        /// </summary>
        public void FadeToScreen(CanvasGroup screen)
        {
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            _transitionCoroutine = StartCoroutine(FadeTransition(screen));
        }

        private IEnumerator FadeTransition(CanvasGroup target)
        {
            // Fade out current screen
            if (_currentScreen != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(_currentScreen, 1f, 0f));
                SetScreenActive(_currentScreen, false);
            }

            // Fade in new screen
            if (target != null)
            {
                target.alpha = 0f;
                SetScreenActive(target, true);
                yield return StartCoroutine(FadeCanvasGroup(target, 0f, 1f));
                _currentScreen = target;
            }

            _transitionCoroutine = null;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to)
        {
            float elapsed = 0f;
            cg.alpha = from;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
                yield return null;
            }

            cg.alpha = to;
        }

        private void SetScreenActive(CanvasGroup cg, bool active)
        {
            cg.gameObject.SetActive(active);
            cg.interactable = active;
            cg.blocksRaycasts = active;
            if (active) cg.alpha = 1f;
        }

        private void HideAllImmediate()
        {
            CanvasGroup[] screens = {
                _splashScreen, _mainMenuScreen, _gameScreen,
                _gameOverScreen, _leaderboardScreen, _howToPlayScreen,
                _settingsScreen
            };

            foreach (var screen in screens)
            {
                if (screen != null)
                {
                    SetScreenActive(screen, false);
                }
            }

            _currentScreen = null;
        }
    }
}
