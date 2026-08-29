using System.Collections.Generic;

namespace LexiGrid.DataStructures
{
    /// <summary>
    /// Represents a single node in a Trie (prefix tree).
    /// </summary>
    public class TrieNode
    {
        private Dictionary<char, TrieNode> _children;

        /// <summary>
        /// Gets the children of this node.
        /// </summary>
        public IReadOnlyDictionary<char, TrieNode> Children => _children;

        /// <summary>
        /// Gets or sets a value indicating whether this node represents the end of a valid word.
        /// </summary>
        public bool IsEndOfWord { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrieNode"/> class.
        /// </summary>
        public TrieNode()
        {
            _children = new Dictionary<char, TrieNode>();
            IsEndOfWord = false;
        }

        /// <summary>
        /// Checks if a child exists for the given character.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if a child exists; otherwise, false.</returns>
        public bool HasChild(char c)
        {
            return _children.ContainsKey(c);
        }

        /// <summary>
        /// Gets the child node for the given character.
        /// </summary>
        /// <param name="c">The character to get the child for.</param>
        /// <returns>The child node if it exists; otherwise, null.</returns>
        public TrieNode GetChild(char c)
        {
            if (_children.TryGetValue(c, out TrieNode node))
            {
                return node;
            }
            return null;
        }

        /// <summary>
        /// Creates and returns a new child node for the given character.
        /// If it already exists, returns the existing one.
        /// </summary>
        /// <param name="c">The character to add.</param>
        /// <returns>The newly created or existing child node.</returns>
        public TrieNode AddChild(char c)
        {
            if (!_children.TryGetValue(c, out TrieNode node))
            {
                node = new TrieNode();
                _children[c] = node;
            }
            return node;
        }
    }
}
