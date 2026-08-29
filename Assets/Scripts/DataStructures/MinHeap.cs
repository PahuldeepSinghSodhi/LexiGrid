using System;
using System.Collections.Generic;

namespace LexiGrid.DataStructures
{
    /// <summary>
    /// A generic minimum heap (priority queue) implementation.
    /// </summary>
    /// <typeparam name="T">The type of elements in the heap, must implement IComparable.</typeparam>
    public class MinHeap<T> where T : IComparable<T>
    {
        private List<T> _items;

        /// <summary>
        /// Gets the number of items in the heap.
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// Gets a value indicating whether the heap is empty.
        /// </summary>
        public bool IsEmpty => _items.Count == 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinHeap{T}"/> class.
        /// </summary>
        public MinHeap()
        {
            _items = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinHeap{T}"/> class with existing items.
        /// </summary>
        /// <param name="items">The items to initialize the heap with.</param>
        public MinHeap(IEnumerable<T> items)
        {
            _items = new List<T>(items);
            for (int i = _items.Count / 2 - 1; i >= 0; i--)
            {
                BubbleDown(i);
            }
        }

        /// <summary>
        /// Inserts an item into the heap and bubbles it up. O(log N) operation.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        public void Insert(T item)
        {
            _items.Add(item);
            BubbleUp(_items.Count - 1);
        }

        /// <summary>
        /// Returns the minimum element without removing it.
        /// </summary>
        /// <returns>The minimum element.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the heap is empty.</exception>
        public T Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Heap is empty.");
            
            return _items[0];
        }

        /// <summary>
        /// Removes and returns the minimum element. O(log N) operation.
        /// </summary>
        /// <returns>The minimum element.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the heap is empty.</exception>
        public T ExtractMin()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Heap is empty.");

            T min = _items[0];
            _items[0] = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);
            
            if (!IsEmpty)
            {
                BubbleDown(0);
            }
            
            return min;
        }

        /// <summary>
        /// Attempts to insert an item if the heap is not full, or if the item is greater than the current minimum.
        /// </summary>
        /// <param name="item">The item to evaluate and potentially insert.</param>
        /// <param name="maxSize">The maximum allowed capacity of the heap.</param>
        /// <returns>True if the item was inserted, false otherwise.</returns>
        public bool TryInsertIfBetter(T item, int maxSize)
        {
            if (Count < maxSize)
            {
                Insert(item);
                return true;
            }

            if (Count >= maxSize && item.CompareTo(Peek()) > 0)
            {
                ExtractMin();
                Insert(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a new array of all items sorted from highest to lowest.
        /// Does not modify the current heap.
        /// </summary>
        /// <returns>A sorted array of items descending.</returns>
        public T[] GetSortedDescending()
        {
            MinHeap<T> copy = new MinHeap<T>(_items);
            List<T> sortedList = new List<T>();
            
            while (!copy.IsEmpty)
            {
                sortedList.Add(copy.ExtractMin());
            }
            
            sortedList.Reverse();
            return sortedList.ToArray();
        }

        /// <summary>
        /// Returns a shallow copy of internal items as an array.
        /// </summary>
        /// <returns>An array containing the heap items.</returns>
        public T[] ToArray()
        {
            return _items.ToArray();
        }

        /// <summary>
        /// Removes all items from the heap.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        private void BubbleUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = Parent(index);
                if (_items[index].CompareTo(_items[parentIndex]) >= 0)
                    break;
                
                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void BubbleDown(int index)
        {
            int lastIndex = _items.Count - 1;
            
            while (true)
            {
                int leftIndex = LeftChild(index);
                int rightIndex = RightChild(index);
                int smallestIndex = index;

                if (leftIndex <= lastIndex && _items[leftIndex].CompareTo(_items[smallestIndex]) < 0)
                {
                    smallestIndex = leftIndex;
                }

                if (rightIndex <= lastIndex && _items[rightIndex].CompareTo(_items[smallestIndex]) < 0)
                {
                    smallestIndex = rightIndex;
                }

                if (smallestIndex == index)
                    break;

                Swap(index, smallestIndex);
                index = smallestIndex;
            }
        }

        private void Swap(int i, int j)
        {
            T temp = _items[i];
            _items[i] = _items[j];
            _items[j] = temp;
        }

        private int Parent(int index) => (index - 1) / 2;
        private int LeftChild(int index) => 2 * index + 1;
        private int RightChild(int index) => 2 * index + 2;
    }
}
