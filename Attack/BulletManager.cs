using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BeardAscetic
{
    public partial class BulletManager : PoolSingleton<BulletManager, BulletBase>, IDataLoader
    {
        public async UniTask LoadDataAsync()
        {
            InitializePool();
            await UniTask.Yield();
        }
        
        public static bool TrySpawnAndLaunch(Vector3 spawnPos, Vector2 direction, float speed, float maxDistance, float radius, bool isPlayerBullet)
        {
            if (Instance.TrySpawn(spawnPos, Quaternion.identity, out BulletBase bullet) == false)
                return false;

            bullet.InitBullet(speed, maxDistance, radius, isPlayerBullet);
            bullet.Launch(direction);
            return true;
        }
    }
}
