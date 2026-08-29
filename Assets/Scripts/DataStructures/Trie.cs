using System.Collections.Generic;
using System.Linq;

namespace LexiGrid.DataStructures
{
    /// <summary>
    /// Represents a Trie (prefix tree) for dictionary validation.
    /// </summary>
    public class Trie
    {
        private TrieNode _root;
        private int _wordCount;

        /// <summary>
        /// Gets the total number of words inserted into the Trie.
        /// </summary>
        public int WordCount => _wordCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Trie"/> class.
        /// </summary>
        public Trie()
        {
            _root = new TrieNode();
            _wordCount = 0;
        }

        /// <summary>
        /// Inserts a word into the trie. Converts to lowercase.
        /// Increments WordCount only if this is a genuinely new word.
        /// Ignores null or empty strings.
        /// </summary>
        /// <param name="word">The word to insert.</param>
        public void Insert(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return;
            }

            word = word.ToLowerInvariant();
            TrieNode current = _root;

            foreach (char c in word)
            {
                current = current.AddChild(c);
            }

            if (!current.IsEndOfWord)
            {
                current.IsEndOfWord = true;
                _wordCount++;
            }
        }

        /// <summary>
        /// Searches for an exact complete word in the trie. Case-insensitive.
        /// </summary>
        /// <param name="word">The word to search for.</param>
        /// <returns>True if the word exists; otherwise, false.</returns>
        public bool Search(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return false;
            }

            word = word.ToLowerInvariant();
            TrieNode current = _root;

            foreach (char c in word)
            {
                if (!current.HasChild(c))
                {
                    return false;
                }
                current = current.GetChild(c);
            }

            return current.IsEndOfWord;
        }

        /// <summary>
        /// Checks if any word in the trie starts with the given prefix. Case-insensitive.
        /// </summary>
        /// <param name="prefix">The prefix to check.</param>
        /// <returns>True if any word starts with the prefix; otherwise, false.</returns>
        public bool StartsWith(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return false;
            }

            prefix = prefix.ToLowerInvariant();
            TrieNode current = _root;

            foreach (char c in prefix)
            {
                if (!current.HasChild(c))
                {
                    return false;
                }
                current = current.GetChild(c);
            }

            return true;
        }

        /// <summary>
        /// Gets all complete words that start with the given prefix.
        /// </summary>
        /// <param name="prefix">The prefix to search for.</param>
        /// <returns>A list of words that start with the prefix.</returns>
        public List<string> GetAllWordsWithPrefix(string prefix)
        {
            List<string> words = new List<string>();

            if (string.IsNullOrEmpty(prefix))
            {
                return words;
            }

            prefix = prefix.ToLowerInvariant();
            TrieNode current = _root;

            foreach (char c in prefix)
            {
                if (!current.HasChild(c))
                {
                    return words;
                }
                current = current.GetChild(c);
            }

            Dfs(current, prefix, words);
            return words;
        }

        private void Dfs(TrieNode node, string currentPrefix, List<string> words)
        {
            if (node.IsEndOfWord)
            {
                words.Add(currentPrefix);
            }

            foreach (var kvp in node.Children)
            {
                Dfs(kvp.Value, currentPrefix + kvp.Key, words);
            }
        }
    }
}
