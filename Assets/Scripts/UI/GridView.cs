using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LexiGrid.Core;

namespace LexiGrid.UI
{
    /// <summary>
    /// Creates and manages the visual grid of Tile objects.
    /// </summary>
    public class GridView : MonoBehaviour
    {
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private Transform _gridParent;
        [SerializeField] private GridLayoutGroup _gridLayout;

        private Tile[,] _tiles;
        private int _gridSize;
        private List<GameObject> _tilePool = new List<GameObject>();

        /// <summary>
        /// Initializes the grid view with the specified size and sets up the pool.
        /// </summary>
        public void Initialize(int gridSize)
        {
            _gridSize = gridSize;
            
            if (_gridLayout != null)
            {
                _gridLayout.constraintCount = gridSize;
            }

            int totalTiles = gridSize * gridSize;
            foreach (GameObject pooled in _tilePool) pooled.SetActive(false);
            for (int i = _tilePool.Count; i < totalTiles; i++)
            {
                GameObject tileObj = Instantiate(_tilePrefab, _gridParent);
                tileObj.SetActive(false);
                _tilePool.Add(tileObj);
            }
            ResizeCells();
        }

        private void OnRectTransformDimensionsChange() => ResizeCells();

        private void ResizeCells()
        {
            if (_gridLayout == null || _gridSize <= 0) return;
            RectTransform rect = _gridLayout.transform as RectTransform;
            float available = Mathf.Min(rect.rect.width - _gridLayout.padding.horizontal,
                rect.rect.height - _gridLayout.padding.vertical);
            float side = Mathf.Max(1, (available - _gridLayout.spacing.x * (_gridSize - 1)) / _gridSize);
            _gridLayout.cellSize = new Vector2(side, side);
        }

        /// <summary>
        /// Populates the grid with letters from the board.
        /// </summary>
        public void PopulateGrid(char[,] board)
        {
            int rows = board.GetLength(0);
            int cols = board.GetLength(1);
            
            if (_tiles == null || _tiles.GetLength(0) != rows || _tiles.GetLength(1) != cols)
            {
                _tiles = new Tile[rows, cols];
            }

            int poolIndex = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GameObject tileGo = GetFromPool(poolIndex++);
                    tileGo.SetActive(true);
                    
                    Tile tile = tileGo.GetComponent<Tile>();
                    tile.Initialize(board[r, c], new GridPosition(r, c));
                    _tiles[r, c] = tile;
                }
            }
        }

        /// <summary>
        /// Gets the tile at the specified row and column.
        /// </summary>
        public Tile GetTile(int row, int col)
        {
            if (row >= 0 && row < _gridSize && col >= 0 && col < _gridSize)
            {
                return _tiles[row, col];
            }
            return null;
        }

        /// <summary>
        /// Resets all tiles in the grid.
        /// </summary>
        public void ResetGrid()
        {
            if (_tiles != null)
            {
                foreach (Tile tile in _tiles)
                {
                    if (tile != null)
                    {
                        tile.FullReset();
                    }
                }
            }
        }

        /// <summary>
        /// Marks the given list of tiles as used.
        /// </summary>
        public void MarkWordTiles(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                tile.SetUsed();
            }
        }

        private GameObject GetFromPool(int index)
        {
            if (index < _tilePool.Count)
            {
                return _tilePool[index];
            }
            
            GameObject newTile = Instantiate(_tilePrefab, _gridParent);
            _tilePool.Add(newTile);
            return newTile;
        }

        private void ReturnToPool(GameObject go)
        {
            go.SetActive(false);
        }
    }
}
