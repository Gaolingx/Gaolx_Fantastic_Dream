using UnityEngine;
using UnityEngine.Pool;

namespace DarkGod.Main
{
    public abstract class BasePool<T> : IBasePool<T> where T : class, new()
    {
        protected GameObject prefab;

        protected ObjectPool<T> pool;

        protected BasePool(GameObject _prefab, bool collectionCheck = true)
        {
            prefab = _prefab;
            pool = new ObjectPool<T>(OnCreatePoolItem, OnGetPoolItem, OnReleasePoolItem, OnDestroyPoolItem, collectionCheck);
        }

        public virtual T Get()
        {
            return pool.Get();
        }

        public virtual void Release(T obj)
        {
            pool.Release(obj);
        }

        public virtual void Clear()
        {
            pool.Clear();
        }

        public virtual T OnCreatePoolItem()
        {
            return null;
        }

        public virtual void OnGetPoolItem(T obj)
        {
        }

        public virtual void OnReleasePoolItem(T obj)
        {
        }

        public virtual void OnDestroyPoolItem(T obj)
        {
        }

        public virtual void OnDestroy()
        {
            pool.Clear();
        }
    }
}
