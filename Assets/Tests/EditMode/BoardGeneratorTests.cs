using NUnit.Framework;
using LexiGrid.Core;
using LexiGrid.DataStructures;
using System.Linq;
using System.Collections.Generic;

namespace LexiGrid.Tests
{
    /// <summary>
    /// NUnit tests for the BoardGenerator.
    /// </summary>
    public class BoardGeneratorTests
    {
        [Test]
        public void GenerateBoard_CorrectDimensions()
        {
            var generator = new BoardGenerator(4);
            var board = generator.GenerateBoard();
            Assert.AreEqual(4, board.GetLength(0));
            Assert.AreEqual(4, board.GetLength(1));
        }

        [Test]
        public void GenerateBoard_AllCellsPopulated()
        {
            var generator = new BoardGenerator(4);
            var board = generator.GenerateBoard();
            
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    Assert.AreNotEqual('\0', board[r, c]);
                }
            }
        }

        [Test]
        public void GenerateBoard_CustomSize()
        {
            var generator = new BoardGenerator(5);
            var board = generator.GenerateBoard();
            Assert.AreEqual(5, board.GetLength(0));
            Assert.AreEqual(5, board.GetLength(1));
        }

        [Test]
        public void GenerateBoard_AllLettersAreUppercase()
        {
            var generator = new BoardGenerator(4);
            var board = generator.GenerateBoard();
            
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    Assert.IsTrue(char.IsUpper(board[r, c]));
                    Assert.IsTrue(char.IsLetter(board[r, c]));
                }
            }
        }

        [Test]
        public void GetAdjacentCells_CenterCell_Returns8()
        {
            var adjacent = BoardGenerator.GetAdjacentCells(2, 2, 5);
            Assert.AreEqual(8, adjacent.Count);
        }

        [Test]
        public void GetAdjacentCells_CornerCell_Returns3()
        {
            var adjacent = BoardGenerator.GetAdjacentCells(0, 0, 4);
            Assert.AreEqual(3, adjacent.Count);
        }

        [Test]
        public void GetAdjacentCells_EdgeCell_Returns5()
        {
            var adjacent = BoardGenerator.GetAdjacentCells(0, 1, 4);
            Assert.AreEqual(5, adjacent.Count);
        }

        [Test]
        public void HasValidWord_WithLoadedTrie_ReturnsTrue()
        {
            var trie = new Trie();
            trie.Insert("CAT");
            
            var generator = new BoardGenerator(4);
            char[,] board = new char[4, 4];
            
            // Fill with placeholder
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    board[r, c] = 'X';
                }
            }
            
            // Set up valid word "CAT" adjacently
            board[0, 0] = 'C';
            board[0, 1] = 'A';
            board[0, 2] = 'T';

            Assert.IsTrue(generator.HasValidWord(board, trie));
        }

        [Test]
        public void HasValidWord_EmptyTrie_ReturnsFalse()
        {
            var trie = new Trie();
            var generator = new BoardGenerator(4);
            var board = generator.GenerateBoard();
            
            Assert.IsFalse(generator.HasValidWord(board, trie));
        }

        [Test]
        public void GenerateSolvableBoard_ReturnsSolvableBoard()
        {
            var trie = new Trie();
            trie.Insert("CAT");
            trie.Insert("DOG");
            trie.Insert("BIRD");
            
            var generator = new BoardGenerator(4);
            var board = generator.GenerateSolvableBoard(trie);
            
            Assert.IsTrue(generator.HasValidWord(board, trie));
        }
    }
}
