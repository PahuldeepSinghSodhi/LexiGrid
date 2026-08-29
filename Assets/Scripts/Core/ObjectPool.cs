using UnityEngine;
using System.Collections.Generic;

namespace LexiGrid.Core
{
    /// <summary>
    /// A generic, reusable object pool for Unity GameObjects.
    /// </summary>
    /// <typeparam name="T">The component type to pool.</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private T _prefab;
        private Transform _parent;
        private Queue<T> _pool = new Queue<T>();
        private List<T> _activeObjects = new List<T>();

        /// <summary>
        /// Initializes a new instance of the ObjectPool class.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="parent">The parent transform for pooled objects.</param>
        /// <param name="initialSize">The initial number of objects to create.</param>
        public ObjectPool(T prefab, Transform parent, int initialSize = 0)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                T newObj = Object.Instantiate(_prefab, _parent);
                newObj.gameObject.SetActive(false);
                _pool.Enqueue(newObj);
            }
        }

        /// <summary>
        /// Gets an object from the pool, instantiating a new one if necessary.
        /// </summary>
        /// <returns>An active object of type T.</returns>
        public T Get()
        {
            T obj;
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = Object.Instantiate(_prefab, _parent);
            }

            obj.gameObject.SetActive(true);
            _activeObjects.Add(obj);
            return obj;
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void Return(T obj)
        {
            if (_activeObjects.Contains(obj))
            {
                obj.gameObject.SetActive(false);
                _activeObjects.Remove(obj);
                _pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// Returns all active objects to the pool.
        /// </summary>
        public void ReturnAll()
        {
            // Reverse loop to avoid modifying the list while iterating
            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                Return(_activeObjects[i]);
            }
        }

        /// <summary>
        /// The number of active objects.
        /// </summary>
        public int ActiveCount => _activeObjects.Count;

        /// <summary>
        /// The number of objects currently in the pool.
        /// </summary>
        public int PooledCount => _pool.Count;
    }
}
