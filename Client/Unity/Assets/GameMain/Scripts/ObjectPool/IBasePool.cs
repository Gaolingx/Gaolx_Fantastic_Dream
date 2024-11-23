namespace DarkGod.Main
{
    public interface IBasePool<T> where T : class, new()
    {
        T Get();

        void Release(T obj);

        void Clear();

        T OnCreatePoolItem();

        void OnGetPoolItem(T obj);

        void OnReleasePoolItem(T obj);

        void OnDestroyPoolItem(T obj);

        void OnDestroy();
    }
}
