using System;
using System.Collections;
using UnityEngine;
using LexiGrid.DataStructures;

namespace LexiGrid.Core
{
    /// <summary>
    /// Loads the word list and manages the Trie.
    /// </summary>
    public class DictionaryManager
    {
        private Trie _trie;
        private bool _isLoaded;

        public Trie Dictionary => _trie;
        public bool IsLoaded => _isLoaded;
        public int WordCount => _trie?.WordCount ?? 0;

        /// <summary>
        /// Initializes a new DictionaryManager.
        /// </summary>
        public DictionaryManager()
        {
            _trie = new Trie();
            _isLoaded = false;
        }

        /// <summary>
        /// Synchronously loads the dictionary from a TextAsset.
        /// </summary>
        public void LoadFromTextAsset(TextAsset asset)
        {
            if (asset == null) return;
            LoadFromString(asset.text);
        }

        /// <summary>
        /// Synchronously loads the dictionary from a raw string.
        /// </summary>
        public void LoadFromString(string content)
        {
            string[] words = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                string cleanWord = word.Trim();
                if (cleanWord.Length >= 3 && IsAlphabetic(cleanWord))
                {
                    _trie.Insert(cleanWord.ToLowerInvariant());
                }
            }
            _isLoaded = true;
        }

        /// <summary>
        /// Asynchronously loads the dictionary from a TextAsset using a coroutine.
        /// </summary>
        public IEnumerator LoadFromTextAssetAsync(TextAsset asset, Action<float> onProgress = null)
        {
            if (asset == null)
            {
                _isLoaded = true;
                yield break;
            }

            string[] words = asset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int totalWords = words.Length;
            int batchSize = 1000;

            for (int i = 0; i < totalWords; i++)
            {
                string cleanWord = words[i].Trim();
                if (cleanWord.Length >= 3 && IsAlphabetic(cleanWord))
                {
                    _trie.Insert(cleanWord.ToLowerInvariant());
                }

                if (i % batchSize == 0 && i > 0)
                {
                    onProgress?.Invoke((float)i / totalWords);
                    yield return null;
                }
            }

            onProgress?.Invoke(1.0f);
            _isLoaded = true;
        }

        private bool IsAlphabetic(string word)
        {
            foreach (char c in word)
            {
                if (!char.IsLetter(c)) return false;
            }
            return true;
        }
    }
}
