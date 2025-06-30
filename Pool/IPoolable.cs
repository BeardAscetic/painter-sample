namespace BeardAscetic
{
    public interface IPoolable
    {
        public void OnSpawnFromPool();

        public void OnReturnToPool();
        bool IsPooled { get; }
    }
}