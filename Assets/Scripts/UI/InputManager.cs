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
        private bool _dragMoved;
        private Tile _downTile;
        private bool _downSelected;
        private Trie _trie;
        private Camera _mainCamera;
        
        [SerializeField] private GraphicRaycaster _graphicRaycaster;
        private EventSystem _eventSystem;

        public event System.Action<string> OnWordSubmitted;
        public event System.Action<string, bool> OnPathUpdated;
        public event System.Action OnSelectionCleared;
        public event System.Action<string> OnWordRejected;

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
                _graphicRaycaster = FindAnyObjectByType<GraphicRaycaster>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || _isDragging) return;
            _isDragging = true;
            _dragMoved = false;
            _downTile = GetTileUnderPointer(eventData);
            _downSelected = _selectedTiles.Contains(_downTile);
            if (_downTile == null) { _isDragging = false; return; }
            if (!_downSelected)
            {
                if (_selectedTiles.Count > 0 && !IsAdjacent(_selectedTiles[_selectedTiles.Count - 1], _downTile)) ClearSelection();
                ProcessTile(_downTile);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            if (!_dragMoved && _downSelected) ProcessTile(_downTile);
            _dragMoved = true;
            ProcessTile(GetTileUnderPointer(eventData));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;
            if (!_dragMoved)
            {
                if (_downSelected) TrimSelection(_selectedTiles.IndexOf(_downTile));
                return;
            }
            SubmitSelection();
        }

        public void SubmitSelection()
        {
            _isDragging = false;

            string word = GetCurrentWord();
            ClearSelection();
            if (word.Length >= ScoringEngine.MinWordLength && _trie != null && _trie.Search(word))
            {
                OnWordSubmitted?.Invoke(word);
            }
            if (word.Length > 0 && (word.Length < ScoringEngine.MinWordLength || _trie == null || !_trie.Search(word)))
                OnWordRejected?.Invoke(word.Length < ScoringEngine.MinWordLength ? "Use at least 3 letters" : "Not in the dictionary. Try another path.");
        }

        private void ProcessTile(Tile tile)
        {
            if (tile == null || tile.IsUsed) return;
            int selected = _selectedTiles.IndexOf(tile);
            if (selected >= 0) { TrimSelection(selected + 1); return; }

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
            UpdatePath();
        }

        private void TrimSelection(int keep)
        {
            if (keep < 0) return;
            for (int i = _selectedTiles.Count - 1; i >= keep; i--)
            { _selectedTiles[i].ResetVisuals(); _selectedTiles.RemoveAt(i); }
            UpdatePath();
        }

        public void UndoLastSelection() => TrimSelection(Mathf.Max(0, _selectedTiles.Count - 1));

        private void UpdatePath()
        {
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

        public void CancelSelection()
        {
            _isDragging = false;
            ClearSelection();
        }

        private void OnDisable() => CancelSelection();
        private void OnApplicationFocus(bool focused) { if (!focused && _isDragging) CancelSelection(); }
        private void OnApplicationPause(bool paused) { if (paused && _isDragging) CancelSelection(); }
    }
}
