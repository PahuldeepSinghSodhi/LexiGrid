using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LexiGrid.Core;
using LexiGrid.DataStructures;

namespace LexiGrid.UI
{
    /// <summary>
    /// Displays the top scores with a back button for navigation.
    /// </summary>
    public class LeaderboardView : MonoBehaviour
    {
        [SerializeField] private Transform _entriesParent;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _backButton;

        private void OnEnable()
        {
            _backButton?.onClick.AddListener(OnBackClicked);
        }

        private void OnDisable()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
        }

        /// <summary>
        /// Displays the given leaderboard entries.
        /// </summary>
        public void DisplayLeaderboard(LeaderboardEntry[] sortedEntries)
        {
            // Clear existing entries
            if (_entriesParent != null)
            {
                foreach (Transform child in _entriesParent)
                {
                    Destroy(child.gameObject);
                }
            }

            if (sortedEntries == null || _entryPrefab == null || _entriesParent == null) return;

            for (int i = 0; i < sortedEntries.Length; i++)
            {
                GameObject entryObj = Instantiate(_entryPrefab, _entriesParent);
                TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{i + 1}";
                    texts[1].text = sortedEntries[i].PlayerName;
                    texts[2].text = sortedEntries[i].Score.ToString();
                }
            }
        }

        /// <summary>
        /// Shows the leaderboard view.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the leaderboard view.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnBackClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
        }
    }
}
