using System;
using System.Collections.Generic;
using LexiGrid.DataStructures;

namespace LexiGrid.Core
{
    /// <summary>
    /// Represents a position on the grid.
    /// </summary>
    public struct GridPosition
    {
        public int Row;
        public int Col;

        public GridPosition(int row, int col)
        {
            Row = row;
            Col = col;
        }
    }

    /// <summary>
    /// Generates the game board and validates solvability.
    /// </summary>
    public class BoardGenerator
    {
        private readonly int _gridSize;

        /// <summary>
        /// Initializes a new BoardGenerator.
        /// </summary>
        public BoardGenerator(int gridSize = 4)
        {
            _gridSize = gridSize;
        }

        /// <summary>
        /// Generates a random game board.
        /// </summary>
        public char[,] GenerateBoard()
        {
            char[,] board = new char[_gridSize, _gridSize];
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    board[r, c] = LetterFrequency.GetWeightedRandomLetter();
                }
            }
            return board;
        }

        /// <summary>
        /// Checks if the board contains at least one valid word.
        /// </summary>
        public bool HasValidWord(char[,] grid, Trie trie, int minWordLength = 3)
        {
            for (int r = 0; r < _gridSize; r++)
            {
                for (int c = 0; c < _gridSize; c++)
                {
                    bool[,] visited = new bool[_gridSize, _gridSize];
                    if (DFS(grid, trie, r, c, "", visited, minWordLength))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool DFS(char[,] grid, Trie trie, int r, int c, string currentPath, bool[,] visited, int minWordLength)
        {
            visited[r, c] = true;
            currentPath += grid[r, c];

            if (!trie.StartsWith(currentPath))
            {
                visited[r, c] = false;
                return false;
            }

            if (currentPath.Length >= minWordLength && trie.Search(currentPath))
            {
                return true;
            }

            var adjacent = GetAdjacentCells(r, c, _gridSize);
            foreach (var pos in adjacent)
            {
                if (!visited[pos.Row, pos.Col])
                {
                    if (DFS(grid, trie, pos.Row, pos.Col, currentPath, visited, minWordLength))
                    {
                        return true;
                    }
                }
            }

            visited[r, c] = false;
            return false;
        }

        /// <summary>
        /// Generates a board guaranteed to have at least one valid word.
        /// </summary>
        public char[,] GenerateSolvableBoard(Trie trie, int maxAttempts = 100)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                char[,] board = GenerateBoard();
                if (HasValidWord(board, trie))
                {
                    return board;
                }
            }
            
            if (!trie.TryFindWord(3, _gridSize * _gridSize, out string guaranteedWord))
                throw new InvalidOperationException("The dictionary has no word that fits this board.");
            char[,] fallback = GenerateBoard();
            for (int i = 0; i < guaranteedWord.Length; i++)
            {
                int row = i / _gridSize;
                int col = row % 2 == 0 ? i % _gridSize : _gridSize - 1 - i % _gridSize;
                fallback[row, col] = char.ToUpperInvariant(guaranteedWord[i]);
            }
            return fallback;
        }

        /// <summary>
        /// Gets all valid adjacent cells for a given position.
        /// </summary>
        public static List<GridPosition> GetAdjacentCells(int row, int col, int gridSize)
        {
            var cells = new List<GridPosition>();
            for (int r = Math.Max(0, row - 1); r <= Math.Min(gridSize - 1, row + 1); r++)
            {
                for (int c = Math.Max(0, col - 1); c <= Math.Min(gridSize - 1, col + 1); c++)
                {
                    if (r == row && c == col) continue;
                    cells.Add(new GridPosition(r, c));
                }
            }
            return cells;
        }
    }
}
