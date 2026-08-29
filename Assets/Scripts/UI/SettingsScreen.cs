using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LexiGrid.Core;

namespace LexiGrid.UI
{
    /// <summary>
    /// Settings screen — grid size and round duration selection with PlayerPrefs persistence.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
        [Header("Grid Size")]
        [SerializeField] private Button _grid3x3Button;
        [SerializeField] private Button _grid4x4Button;
        [SerializeField] private Button _grid5x5Button;
        [SerializeField] private Image _grid3x3Bg;
        [SerializeField] private Image _grid4x4Bg;
        [SerializeField] private Image _grid5x5Bg;

        [Header("Round Duration")]
        [SerializeField] private Button _duration30Button;
        [SerializeField] private Button _duration60Button;
        [SerializeField] private Button _duration90Button;
        [SerializeField] private Button _duration120Button;
        [SerializeField] private Image _duration30Bg;
        [SerializeField] private Image _duration60Bg;
        [SerializeField] private Image _duration90Bg;
        [SerializeField] private Image _duration120Bg;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        // PlayerPrefs keys
        private const string GridSizeKey = "LexiGrid_GridSize";
        private const string DurationKey = "LexiGrid_Duration";

        // Colors
        private static readonly Color SelectedColor = new Color(0.29f, 0.62f, 1f);    // #4A9EFF
        private static readonly Color UnselectedColor = new Color(0.2f, 0.22f, 0.28f); // #333846

        private int _selectedGridSize;
        private int _selectedDuration;

        private void OnEnable()
        {
            _selectedGridSize = PlayerPrefs.GetInt(GridSizeKey, 4);
            _selectedDuration = PlayerPrefs.GetInt(DurationKey, 60);

            _grid3x3Button?.onClick.AddListener(() => SetGridSize(3));
            _grid4x4Button?.onClick.AddListener(() => SetGridSize(4));
            _grid5x5Button?.onClick.AddListener(() => SetGridSize(5));

            _duration30Button?.onClick.AddListener(() => SetDuration(30));
            _duration60Button?.onClick.AddListener(() => SetDuration(60));
            _duration90Button?.onClick.AddListener(() => SetDuration(90));
            _duration120Button?.onClick.AddListener(() => SetDuration(120));

            _backButton?.onClick.AddListener(OnBackClicked);

            UpdateVisuals();
        }

        private void OnDisable()
        {
            _grid3x3Button?.onClick.RemoveAllListeners();
            _grid4x4Button?.onClick.RemoveAllListeners();
            _grid5x5Button?.onClick.RemoveAllListeners();

            _duration30Button?.onClick.RemoveAllListeners();
            _duration60Button?.onClick.RemoveAllListeners();
            _duration90Button?.onClick.RemoveAllListeners();
            _duration120Button?.onClick.RemoveAllListeners();

            _backButton?.onClick.RemoveAllListeners();
        }

        private void SetGridSize(int size)
        {
            _selectedGridSize = size;
            PlayerPrefs.SetInt(GridSizeKey, size);
            PlayerPrefs.Save();
            UpdateVisuals();
        }

        private void SetDuration(int seconds)
        {
            _selectedDuration = seconds;
            PlayerPrefs.SetInt(DurationKey, seconds);
            PlayerPrefs.Save();
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // Grid size buttons
            SetButtonColor(_grid3x3Bg, _selectedGridSize == 3);
            SetButtonColor(_grid4x4Bg, _selectedGridSize == 4);
            SetButtonColor(_grid5x5Bg, _selectedGridSize == 5);

            // Duration buttons
            SetButtonColor(_duration30Bg, _selectedDuration == 30);
            SetButtonColor(_duration60Bg, _selectedDuration == 60);
            SetButtonColor(_duration90Bg, _selectedDuration == 90);
            SetButtonColor(_duration120Bg, _selectedDuration == 120);
        }

        private void SetButtonColor(Image bg, bool selected)
        {
            if (bg != null)
            {
                bg.color = selected ? SelectedColor : UnselectedColor;
            }
        }

        private void OnBackClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
        }

        /// <summary>
        /// Gets the saved grid size from PlayerPrefs.
        /// </summary>
        public static int GetSavedGridSize()
        {
            return PlayerPrefs.GetInt(GridSizeKey, 4);
        }

        /// <summary>
        /// Gets the saved round duration from PlayerPrefs.
        /// </summary>
        public static int GetSavedDuration()
        {
            return PlayerPrefs.GetInt(DurationKey, 60);
        }
    }
}
