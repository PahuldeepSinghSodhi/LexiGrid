using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LexiGrid.Core;

namespace LexiGrid.UI
{
    /// <summary>
    /// MonoBehaviour representing a single letter tile on the grid.
    /// </summary>
    public class Tile : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _letterText;
        [SerializeField] private Image _backgroundImage;

        private char _letter;
        private GridPosition _gridPosition;
        private bool _isUsed;
        private bool _isSelected;

        public char Letter => _letter;
        public GridPosition GridPosition => _gridPosition;
        public bool IsUsed => _isUsed;
        public bool IsSelected => _isSelected;

        private static readonly Color DefaultColor = new Color(0.14f, 0.19f, 0.27f);
        private static readonly Color SelectedColor = new Color(0.25f, 0.39f, 0.48f);
        private static readonly Color ValidColor = new Color(0.2f, 0.78f, 0.62f);
        private static readonly Color InvalidColor = new Color(0.75f, 0.24f, 0.33f);
        private static readonly Color UsedColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);

        /// <summary>
        /// Initializes the tile with a letter and position.
        /// </summary>
        public void Initialize(char letter, GridPosition position)
        {
            _letter = letter;
            _gridPosition = position;
            _letterText.text = letter.ToString().ToUpper();
            ResetVisuals();
            _isUsed = false;
        }

        /// <summary>
        /// Sets the selected state and updates visual appearance.
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateColor();
        }

        /// <summary>
        /// Sets the validity state visually.
        /// </summary>
        public void SetValidState(bool isValid)
        {
            _backgroundImage.color = isValid ? ValidColor : InvalidColor;
            _letterText.color = isValid ? new Color(0.03f, 0.12f, 0.14f) : Color.white;
        }

        /// <summary>
        /// Marks the tile as used and makes it non-interactable.
        /// </summary>
        public void SetUsed()
        {
            _isUsed = true;
            _isSelected = false;
            UpdateColor();
        }

        /// <summary>
        /// Resets the tile's visuals without changing its used state.
        /// </summary>
        public void ResetVisuals()
        {
            _isSelected = false;
            UpdateColor();
        }

        /// <summary>
        /// Fully resets the tile's state for a new round.
        /// </summary>
        public void FullReset()
        {
            _isUsed = false;
            ResetVisuals();
        }

        private void UpdateColor()
        {
            _letterText.color = new Color(0.94f, 0.97f, 1f);
            if (_isUsed)
            {
                _backgroundImage.color = UsedColor;
            }
            else if (_isSelected)
            {
                _backgroundImage.color = SelectedColor;
            }
            else
            {
                _backgroundImage.color = DefaultColor;
            }
        }
    }
}
