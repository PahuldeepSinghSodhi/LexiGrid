using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LexiGrid.Core;

namespace LexiGrid.UI
{
    /// <summary>
    /// Splash/loading screen shown on app launch while the dictionary loads.
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private Slider _loadingBar;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private TextMeshProUGUI _titleText;

        /// <summary>
        /// Updates the loading progress bar and text.
        /// </summary>
        public void UpdateProgress(float progress)
        {
            if (_loadingBar != null)
            {
                _loadingBar.value = progress;
            }

            if (_loadingText != null)
            {
                int percent = Mathf.RoundToInt(progress * 100f);
                _loadingText.text = $"Loading dictionary... {percent}%";
            }
        }

        /// <summary>
        /// Called when loading is complete — shows 100% briefly.
        /// </summary>
        public void OnLoadingComplete()
        {
            UpdateProgress(1f);
            if (_loadingText != null)
            {
                _loadingText.text = "Ready!";
            }
        }
    }
}
