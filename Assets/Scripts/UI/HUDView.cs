using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LexiGrid.UI
{
    /// <summary>
    /// In-game heads-up display.
    /// </summary>
    public class HUDView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _currentWordText;
        [SerializeField] private TextMeshProUGUI _foundWordsText;
        [SerializeField] private TextMeshProUGUI _wordCountText;

        private List<string> _foundWords = new List<string>();

        /// <summary>
        /// Updates the score display.
        /// </summary>
        public void UpdateScore(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {score}";
            }
        }

        /// <summary>
        /// Updates the timer display.
        /// </summary>
        public void UpdateTimer(float secondsRemaining)
        {
            if (_timerText != null)
            {
                int total = Mathf.CeilToInt(Mathf.Max(0, secondsRemaining));
                int minutes = total / 60;
                int seconds = total % 60;
                _timerText.text = $"{minutes:00}:{seconds:00}";
                
                _timerText.color = secondsRemaining < 10f ? new Color(1f, 0.45f, 0.48f) : Color.white;
            }
        }

        /// <summary>
        /// Updates the current word being dragged.
        /// </summary>
        public void UpdateCurrentWord(string word, bool isValidPrefix)
        {
            if (_currentWordText != null)
            {
                _currentWordText.text = string.IsNullOrEmpty(word) ? "Drag across letters to build a word" : word;
                _currentWordText.color = isValidPrefix ? new Color(0.4f, 0.92f, 0.76f) : new Color(1f, 0.45f, 0.48f);
            }
        }

        public void ShowFeedback(string message)
        {
            if (_currentWordText != null) _currentWordText.text = message;
        }

        /// <summary>
        /// Adds a found word to the history.
        /// </summary>
        public void AddFoundWord(string word, int pointsEarned)
        {
            _foundWords.Add($"{word} (+{pointsEarned})");
            
            if (_foundWordsText != null)
            {
                int start = Mathf.Max(0, _foundWords.Count - 5);
                _foundWordsText.text = string.Join("\n", _foundWords.GetRange(start, _foundWords.Count - start));
            }

            if (_wordCountText != null)
            {
                _wordCountText.text = $"Words: {_foundWords.Count}";
            }
        }

        /// <summary>
        /// Resets the HUD to initial state.
        /// </summary>
        public void Reset()
        {
            _foundWords.Clear();
            UpdateScore(0);
            UpdateCurrentWord("", true);
            if (_foundWordsText != null) _foundWordsText.text = "";
            if (_wordCountText != null) _wordCountText.text = "Words: 0";
            if (_timerText != null)
            {
                _timerText.text = "00:00";
                _timerText.color = Color.white;
            }
        }
    }
}
