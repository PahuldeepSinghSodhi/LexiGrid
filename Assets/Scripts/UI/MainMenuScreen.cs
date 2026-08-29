using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LexiGrid.Core;

namespace LexiGrid.UI
{
    /// <summary>
    /// Main menu screen with Play, Leaderboard, How to Play, and Settings buttons.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _leaderboardButton;
        [SerializeField] private Button _howToPlayButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TextMeshProUGUI _bestScoreText;

        private void OnEnable()
        {
            _playButton?.onClick.AddListener(OnPlayClicked);
            _leaderboardButton?.onClick.AddListener(OnLeaderboardClicked);
            _howToPlayButton?.onClick.AddListener(OnHowToPlayClicked);
            _settingsButton?.onClick.AddListener(OnSettingsClicked);

            UpdateBestScore();
        }

        private void OnDisable()
        {
            _playButton?.onClick.RemoveListener(OnPlayClicked);
            _leaderboardButton?.onClick.RemoveListener(OnLeaderboardClicked);
            _howToPlayButton?.onClick.RemoveListener(OnHowToPlayClicked);
            _settingsButton?.onClick.RemoveListener(OnSettingsClicked);
        }

        /// <summary>
        /// Updates the best score text from the current leaderboard.
        /// </summary>
        public void UpdateBestScore()
        {
            if (_bestScoreText != null && GameManager.Instance != null)
            {
                int bestScore = GameManager.Instance.GetBestScore();
                _bestScoreText.text = bestScore > 0 ? $"Best Score: {bestScore}" : "";
            }
        }

        private void OnPlayClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewRound();
            }
        }

        private void OnLeaderboardClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowLeaderboardScreen();
            }
        }

        private void OnHowToPlayClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowHowToPlayScreen();
            }
        }

        private void OnSettingsClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowSettingsScreen();
            }
        }
    }
}
