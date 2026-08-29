using System.Collections.Generic;
using NUnit.Framework;
using LexiGrid.DataStructures;

namespace LexiGrid.Tests
{
    public class TrieTests
    {
        private Trie _trie;

        [SetUp]
        public void Setup()
        {
            _trie = new Trie();
        }

        [Test]
        public void Insert_And_Search_SingleWord()
        {
            _trie.Insert("hello");
            Assert.IsTrue(_trie.Search("hello"));
        }

        [Test]
        public void Search_NonExistentWord_ReturnsFalse()
        {
            Assert.IsFalse(_trie.Search("xyz"));
        }

        [Test]
        public void Search_PrefixOnly_ReturnsFalse()
        {
            _trie.Insert("hello");
            Assert.IsFalse(_trie.Search("hell"));
        }

        [Test]
        public void StartsWith_ValidPrefix_ReturnsTrue()
        {
            _trie.Insert("hello");
            Assert.IsTrue(_trie.StartsWith("hell"));
        }

        [Test]
        public void StartsWith_InvalidPrefix_ReturnsFalse()
        {
            _trie.Insert("hello");
            Assert.IsFalse(_trie.StartsWith("xyz"));
        }

        [Test]
        public void Insert_DuplicateWord_DoesNotIncrementCount()
        {
            _trie.Insert("hello");
            _trie.Insert("hello");
            Assert.AreEqual(1, _trie.WordCount);
        }

        [Test]
        public void Insert_NullOrEmpty_IsIgnored()
        {
            _trie.Insert(null);
            _trie.Insert("");
            Assert.AreEqual(0, _trie.WordCount);
        }

        [Test]
        public void Search_IsCaseInsensitive()
        {
            _trie.Insert("Hello");
            Assert.IsTrue(_trie.Search("hello"));
            Assert.IsTrue(_trie.Search("HELLO"));
        }

        [Test]
        public void StartsWith_IsCaseInsensitive()
        {
            _trie.Insert("Hello");
            Assert.IsTrue(_trie.StartsWith("HEL"));
        }

        [Test]
        public void GetAllWordsWithPrefix_ReturnsMatchingWords()
        {
            _trie.Insert("cat");
            _trie.Insert("car");
            _trie.Insert("card");
            _trie.Insert("dog");
            
            var words = _trie.GetAllWordsWithPrefix("car");
            Assert.AreEqual(2, words.Count);
            Assert.Contains("car", words);
            Assert.Contains("card", words);
        }

        [Test]
        public void GetAllWordsWithPrefix_NoMatch_ReturnsEmpty()
        {
            _trie.Insert("cat");
            var words = _trie.GetAllWordsWithPrefix("xyz");
            Assert.IsEmpty(words);
        }

        [Test]
        public void WordCount_TracksCorrectly()
        {
            _trie.Insert("one");
            _trie.Insert("two");
            _trie.Insert("three");
            _trie.Insert("four");
            _trie.Insert("five");
            Assert.AreEqual(5, _trie.WordCount);
        }

        [Test]
        public void Search_SingleCharWord()
        {
            _trie.Insert("a");
            Assert.IsTrue(_trie.Search("a"));
        }

        [Test]
        public void StartsWith_SingleChar()
        {
            _trie.Insert("apple");
            Assert.IsTrue(_trie.StartsWith("a"));
        }

        [Test]
        public void Search_EmptyString_ReturnsFalse()
        {
            Assert.IsFalse(_trie.Search(""));
        }
    }
}
