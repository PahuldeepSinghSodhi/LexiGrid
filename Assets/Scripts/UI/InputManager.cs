using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LexiGrid.Core;
using LexiGrid.DataStructures;

namespace LexiGrid.UI
{
    /// <summary>
    /// Handles swipe/drag input across the grid to form words.
    /// </summary>
    public class InputManager : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private List<Tile> _selectedTiles = new List<Tile>();
        private bool _isDragging;
        private Trie _trie;
        private Camera _mainCamera;
        
        [SerializeField] private GraphicRaycaster _graphicRaycaster;
        private EventSystem _eventSystem;

        public event System.Action<string> OnWordSubmitted;
        public event System.Action<string, bool> OnPathUpdated;
        public event System.Action OnSelectionCleared;

        /// <summary>
        /// Initializes the InputManager with a trie.
        /// </summary>
        public void Initialize(Trie trie)
        {
            _trie = trie;
            _mainCamera = Camera.main;
            _eventSystem = EventSystem.current;
            if (_graphicRaycaster == null)
            {
                _graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            ClearSelection();
            ProcessTileUnderPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            ProcessTileUnderPointer(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;

            string word = GetCurrentWord();
            // In actual usage, get min word length from ScoringEngine, assuming it's statically accessible or configured
            if (word.Length >= 3 && _trie != null && _trie.Search(word)) // Defaulted to 3 for fallback
            {
                OnWordSubmitted?.Invoke(word);
            }
            
            ClearSelection();
        }

        private void ProcessTileUnderPointer(PointerEventData eventData)
        {
            Tile tile = GetTileUnderPointer(eventData);
            if (tile == null || tile.IsUsed) return;

            if (_selectedTiles.Count == 0)
            {
                AddTileToSelection(tile);
            }
            else
            {
                Tile lastTile = _selectedTiles[_selectedTiles.Count - 1];
                if (tile != lastTile && !_selectedTiles.Contains(tile) && IsAdjacent(lastTile, tile))
                {
                    AddTileToSelection(tile);
                }
            }
        }

        private void AddTileToSelection(Tile tile)
        {
            _selectedTiles.Add(tile);
            tile.SetSelected(true);
            
            string currentWord = GetCurrentWord();
            bool isValidPrefix = _trie == null || _trie.StartsWith(currentWord);
            
            foreach(var t in _selectedTiles)
            {
                t.SetValidState(isValidPrefix);
            }

            OnPathUpdated?.Invoke(currentWord, isValidPrefix);
        }

        private Tile GetTileUnderPointer(PointerEventData eventData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            if (_graphicRaycaster != null && _eventSystem != null)
            {
                _graphicRaycaster.Raycast(eventData, results);
            }
            else if (_eventSystem != null)
            {
                _eventSystem.RaycastAll(eventData, results);
            }

            foreach (var result in results)
            {
                Tile tile = result.gameObject.GetComponentInParent<Tile>();
                if (tile != null)
                {
                    return tile;
                }
            }
            return null;
        }

        private bool IsAdjacent(Tile a, Tile b)
        {
            int rowDiff = Mathf.Abs(a.GridPosition.Row - b.GridPosition.Row);
            int colDiff = Mathf.Abs(a.GridPosition.Col - b.GridPosition.Col);
            return rowDiff <= 1 && colDiff <= 1 && (rowDiff != 0 || colDiff != 0);
        }

        private string GetCurrentWord()
        {
            char[] chars = new char[_selectedTiles.Count];
            for (int i = 0; i < _selectedTiles.Count; i++)
            {
                chars[i] = _selectedTiles[i].Letter;
            }
            return new string(chars);
        }

        private void ClearSelection()
        {
            foreach (var tile in _selectedTiles)
            {
                tile.ResetVisuals();
            }
            _selectedTiles.Clear();
            OnSelectionCleared?.Invoke();
        }
    }
}
