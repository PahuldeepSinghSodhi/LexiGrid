using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LexiGrid.Core;

namespace LexiGrid.UI
{
    /// <summary>
    /// Game Over screen — shows final score, words found, new high score indicator,
    /// and Play Again / Main Menu buttons.
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _wordsFoundText;
        [SerializeField] private TextMeshProUGUI _newHighScoreText;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _mainMenuButton;

        private void OnEnable()
        {
            _playAgainButton?.onClick.AddListener(OnPlayAgainClicked);
            _mainMenuButton?.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnDisable()
        {
            _playAgainButton?.onClick.RemoveListener(OnPlayAgainClicked);
            _mainMenuButton?.onClick.RemoveListener(OnMainMenuClicked);
        }

        /// <summary>
        /// Populates the Game Over screen with round results.
        /// </summary>
        public void ShowResults(int finalScore, int wordsFound, bool isNewHighScore)
        {
            if (_finalScoreText != null)
            {
                _finalScoreText.text = finalScore.ToString();
            }

            if (_wordsFoundText != null)
            {
                _wordsFoundText.text = $"Words Found: {wordsFound}";
            }

            if (_newHighScoreText != null)
            {
                _newHighScoreText.gameObject.SetActive(isNewHighScore);
            }
        }

        private void OnPlayAgainClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewRound();
            }
        }

        private void OnMainMenuClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
        }
    }
}
