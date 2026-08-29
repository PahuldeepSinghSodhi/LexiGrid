using UnityEngine;
using UnityEngine.UI;
using LexiGrid.Core;

namespace LexiGrid.UI
{
    /// <summary>
    /// How to Play screen showing game rules and scoring.
    /// </summary>
    public class HowToPlayScreen : MonoBehaviour
    {
        [SerializeField] private Button _backButton;

        private void OnEnable()
        {
            _backButton?.onClick.AddListener(OnBackClicked);
        }

        private void OnDisable()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
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
